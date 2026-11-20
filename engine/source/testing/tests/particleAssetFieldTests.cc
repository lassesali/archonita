//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

// We don't want tests in a shipping version.
#ifndef TORQUE_SHIPPING

#ifndef _UNIT_TESTING_H_
#include "testing/unitTesting.h"
#endif

#ifndef _PARTICLE_ASSET_FIELD_H_
#include "2d/assets/ParticleAssetField.h"
#endif

#ifndef _TAML_CUSTOM_H_
#include "persistence/taml/tamlCustom.h"
#endif

//-----------------------------------------------------------------------------
// A particle field's keys, as a file hands them over
//
// ParticleAssetField::getFieldValue finds the two keys either side of a time by
// walking the keys in order, and takes the first key to be at time 0: a time
// before it has no key on its left, and the search stepped off the front of the
// array. The editor never writes such a field, but nothing stopped a file from
// saying one -- a curve starting at 0.6, or its keys out of order -- and the
// asset loaded without complaint and crashed the first time it was sampled
// early.
//-----------------------------------------------------------------------------

// A field with room on its time axis for keys past 1, as an emitter's base
// fields have.
static void initializeField( ParticleAssetField& field )
{
    field.setFieldName( "SizeX" );
    field.initialize( 10.0f, 0.0f, 100.0f, 1.0f );
}

static void addKey( TamlCustomNode* pFieldNode, const F32 time, const F32 value )
{
    TamlCustomNode* pKeyNode = pFieldNode->addNode( "Key" );
    pKeyNode->addField( "Time", time );
    pKeyNode->addField( "Value", value );
}

TEST( ParticleAssetFieldTests, AFieldWithNoKeyAtTimeZeroGainsOne )
{
    TamlCustomNodes nodes;
    TamlCustomNode* pFieldNode = nodes.addNode( "SizeX" );
    addKey( pFieldNode, 0.6f, 0.4f );
    addKey( pFieldNode, 1.5f, 1.9f );

    ParticleAssetField field;
    initializeField( field );
    field.onTamlCustomRead( pFieldNode, "Unit test asset" );

    ASSERT_EQ( field.getDataKeyCount(), 3 );
    ASSERT_FLOAT_EQ( field.getDataKeyTime( 0 ), 0.0f );
    ASSERT_FLOAT_EQ( field.getDataKeyValue( 0 ), 0.4f ) << "The curve is flat up to its first key.";
    ASSERT_FLOAT_EQ( field.getDataKeyTime( 1 ), 0.6f );
    ASSERT_FLOAT_EQ( field.getDataKeyTime( 2 ), 1.5f );

    SUCCEED();
}

TEST( ParticleAssetFieldTests, ATimeBeforeTheFirstKeyReadsTheFirstKeysValue )
{
    TamlCustomNodes nodes;
    TamlCustomNode* pFieldNode = nodes.addNode( "SizeX" );
    addKey( pFieldNode, 0.6f, 0.4f );
    addKey( pFieldNode, 1.5f, 1.9f );

    ParticleAssetField field;
    initializeField( field );
    field.onTamlCustomRead( pFieldNode, "Unit test asset" );

    ASSERT_FLOAT_EQ( field.getFieldValue( 0.3f ), 0.4f );

    SUCCEED();
}

TEST( ParticleAssetFieldTests, KeysReadOutOfOrderAreSorted )
{
    TamlCustomNodes nodes;
    TamlCustomNode* pFieldNode = nodes.addNode( "SizeX" );
    addKey( pFieldNode, 1.0f, 5.0f );
    addKey( pFieldNode, 0.0f, 1.0f );
    addKey( pFieldNode, 0.5f, 3.0f );

    ParticleAssetField field;
    initializeField( field );
    field.onTamlCustomRead( pFieldNode, "Unit test asset" );

    ASSERT_EQ( field.getDataKeyCount(), 3 );
    ASSERT_FLOAT_EQ( field.getDataKeyTime( 0 ), 0.0f );
    ASSERT_FLOAT_EQ( field.getDataKeyTime( 1 ), 0.5f );
    ASSERT_FLOAT_EQ( field.getDataKeyTime( 2 ), 1.0f );
    ASSERT_FLOAT_EQ( field.getFieldValue( 0.25f ), 2.0f ) << "Halfway between the first two keys, once they are in order.";

    SUCCEED();
}

TEST( ParticleAssetFieldTests, ConstantAndSingleKeyFastPaths )
{
    ParticleAssetField field;
    initializeField( field );

    // Default single key at time 0 with value 1.0f.
    EXPECT_TRUE( field.isConstant() );
    EXPECT_FLOAT_EQ( field.getConstantValue(), 1.0f );
    EXPECT_FLOAT_EQ( field.getFieldValue( -0.5f ), 1.0f );
    EXPECT_FLOAT_EQ( field.getFieldValue( 0.0f ), 1.0f );
    EXPECT_FLOAT_EQ( field.getFieldValue( 3.5f ), 1.0f );
    EXPECT_FLOAT_EQ( field.getFieldValue( 99.0f ), 1.0f );

    EXPECT_TRUE( field.setValueScale( 2.5f ) );
    EXPECT_TRUE( field.isConstant() );
    EXPECT_FLOAT_EQ( field.getConstantValue(), 2.5f );
    EXPECT_FLOAT_EQ( field.getFieldValue( 5.0f ), 2.5f );

    // Multiple keys with identical values still collapse to constant.
    EXPECT_GE( field.addDataKey( 2.0f, 1.0f ), 0 );
    EXPECT_GE( field.addDataKey( 5.0f, 1.0f ), 0 );
    EXPECT_TRUE( field.isConstant() );
    EXPECT_FLOAT_EQ( field.getFieldValue( 4.0f ), 2.5f );

    // Adding a differing key disables the constant fast-path and interpolates cleanly.
    EXPECT_GE( field.addDataKey( 4.0f, 3.0f ), 0 );
    EXPECT_FALSE( field.isConstant() );
    EXPECT_FLOAT_EQ( field.getFieldValue( 2.0f ), 2.5f );
    EXPECT_FLOAT_EQ( field.getFieldValue( 3.0f ), 5.0f );
    EXPECT_FLOAT_EQ( field.getFieldValue( 4.0f ), 7.5f );
    EXPECT_FLOAT_EQ( field.getFieldValue( 5.0f ), 2.5f );
    EXPECT_FLOAT_EQ( field.getFieldValue( 10.0f ), 2.5f );
}

TEST( ParticleAssetFieldTests, SegmentCacheInvalidatesOnAllMutations )
{
    ParticleAssetField field;
    field.setFieldName( "TestCurve" );
    field.initialize( 10.0f, 0.0f, 100.0f, 1.0f );

    // Build 11 keys (10 segments) so the binary-search path (> 8 segments) is exercised.
    for ( U32 i = 1; i <= 10; ++i )
    {
        const F32 t = (F32)i;
        const F32 v = (F32)(i * 2 + 1);
        ASSERT_GE( field.addDataKey( t, v ), 0 );
    }
    EXPECT_FALSE( field.isConstant() );

    // Linear sample inside binary-search path: time 6.25 is 25% from t=6 (val=13) to t=7 (val=15).
    EXPECT_NEAR( field.getFieldValue( 6.25f ), 13.5f, 1e-5f );

    // Mutate an existing key via setDataKeyValue: t=7 goes from 15 to 21.
    EXPECT_TRUE( field.setDataKeyValue( 7, 21.0f ) );
    EXPECT_NEAR( field.getFieldValue( 6.25f ), 15.0f, 1e-5f );

    // Scale and repeat time updates must reflect immediately.
    EXPECT_TRUE( field.setValueScale( 0.5f ) );
    EXPECT_NEAR( field.getFieldValue( 6.25f ), 7.5f, 1e-5f );

    EXPECT_TRUE( field.setRepeatTime( 2.0f ) );
    // At time=6.25 * 2 = 12.5; fmod(12.5, 10 + eps) ~= 2.5 -> t=2 (val=5), t=3 (val=7) -> 6.0 * 0.5 = 3.0.
    EXPECT_NEAR( field.getFieldValue( 6.25f ), 3.0f, 1e-4f );

    // Zero repeat time wraps non-zero sample times to 0.
    EXPECT_TRUE( field.setRepeatTime( 0.0f ) );
    EXPECT_FLOAT_EQ( field.getFieldValue( 6.25f ), 0.5f );

    EXPECT_TRUE( field.setRepeatTime( 1.0f ) );
    EXPECT_TRUE( field.setValueScale( 1.0f ) );

    // copyTo must copy repeatTime, valueScale, keys, and rebuild destination cache.
    ParticleAssetField copyTarget;
    copyTarget.setFieldName( "CopyTarget" );
    copyTarget.initialize( 1.0f, 0.0f, 1.0f, 0.0f );
    field.copyTo( copyTarget );
    EXPECT_NEAR( copyTarget.getFieldValue( 6.25f ), 15.0f, 1e-5f );

    // removeDataKey removes key 7 (time 7, value 21) -> segment now goes t=6 (13) to t=8 (17).
    EXPECT_TRUE( field.removeDataKey( 7 ) );
    EXPECT_NEAR( field.getFieldValue( 7.0f ), 15.0f, 1e-5f );

    // setSingleDataKey and resetDataKeys must collapse back to constant.
    EXPECT_EQ( field.setSingleDataKey( 4.0f ), 0 );
    EXPECT_TRUE( field.isConstant() );
    EXPECT_FLOAT_EQ( field.getFieldValue( 8.0f ), 4.0f );

    field.resetDataKeys();
    EXPECT_TRUE( field.isConstant() );
    EXPECT_FLOAT_EQ( field.getFieldValue( 8.0f ), 1.0f );
}

TEST( ParticleAssetFieldTests, ZeroVariationFieldCalculationIsDeterministic )
{
    ParticleAssetField base;
    base.setFieldName( "Base" );
    base.initialize( 10.0f, -5.0f, 5.0f, 2.0f );
    base.setSingleDataKey( 3.0f );

    ParticleAssetField variation;
    variation.setFieldName( "Variation" );
    variation.initialize( 10.0f, 0.0f, 10.0f, 0.0f );

    ParticleAssetField effect;
    effect.setFieldName( "Effect" );
    effect.initialize( 10.0f, 0.0f, 10.0f, 2.0f );
    effect.setSingleDataKey( 2.0f );

    ParticleAssetField life;
    life.setFieldName( "Life" );
    life.initialize( 1.0f, 0.0f, 10.0f, 1.0f );
    life.setSingleDataKey( 0.5f );

    EXPECT_FLOAT_EQ( ParticleAssetField::calculateFieldBV( base, variation, 1.0f ), 3.0f );
    // 3.0 * 2.0 = 6.0 -> clamped to Base maxValue (5.0).
    EXPECT_FLOAT_EQ( ParticleAssetField::calculateFieldBVE( base, variation, effect, 1.0f ), 5.0f );
    // 3.0 * 2.0 = 6.0 -> modulated by 4.0 = 2.0.
    EXPECT_FLOAT_EQ( ParticleAssetField::calculateFieldBVE( base, variation, effect, 1.0f, true, 4.0f ), 2.0f );
    // 3.0 * 2.0 * 0.5 = 3.0.
    EXPECT_FLOAT_EQ( ParticleAssetField::calculateFieldBVLE( base, variation, life, effect, 1.0f, 0.5f ), 3.0f );
}

#endif // TORQUE_SHIPPING
