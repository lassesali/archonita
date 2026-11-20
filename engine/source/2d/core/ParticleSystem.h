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

#ifndef _PARTICLE_SYSTEM_H_
#define _PARTICLE_SYSTEM_H_

#ifndef _VECTOR2_H_
#include "2d/core/Vector2.h"
#endif

#ifndef _GCOLOR_H_
#include "graphics/gColor.h"
#endif

//-----------------------------------------------------------------------------

class ParticleSystem
{
public:
    /// Compact, trivially destructible particle state laid out with render-hot fields first.
    struct ParticleNode
    {
        // Render-hot fields (48 bytes — single cache line)
        Vector2     mPosition;           //  0..7
        Vector2     mRenderSize;         //  8..15
        ColorF      mColor;              // 16..31
        F32         mOrientationAngle;   // 32..35
        S32         mFrameIndex;         // 36..39 (random static image frame or validated animation frame index)
        Vector2     mPreTickPosition;    // 40..47

        // Simulation fields (48 bytes)
        Vector2     mVelocity;           // 48..55
        Vector2     mSize;               // 56..63
        F32         mParticleAge;        // 64..67
        F32         mParticleLifetime;   // 68..71
        F32         mSpeed;              // 72..75
        F32         mSpin;               // 76..79
        F32         mFixedForce;         // 80..83
        F32         mRandomMotion;       // 84..87
        F32         mAnimationTime;      // 88..91
        bool        mAnimationFinished;  // 92 (+3 pad -> 96 bytes total)
    };

private:
    U32                     mActiveParticleCount;
    U32                     mAllocatedParticleCount;

public:
    static void Init( void );
    static void destroy( void );
    static ParticleSystem* Instance;

    ParticleSystem();
    ~ParticleSystem();

    void adjustActiveParticleCount( const S32 delta );
    void adjustAllocatedParticleCount( const S32 delta );

    inline U32 getActiveParticleCount( void ) const { return mActiveParticleCount; }
    inline U32 getAllocatedParticleCount( void ) const { return mAllocatedParticleCount; }
};

#endif // _PARTICLE_SYSTEM_H_
