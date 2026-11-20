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

#include "2d/core/ParticleSystem.h"

//------------------------------------------------------------------------------

ParticleSystem* ParticleSystem::Instance = NULL;

//------------------------------------------------------------------------------

void ParticleSystem::Init( void )
{
    // Create the particle system.
    Instance = new ParticleSystem();
}

//------------------------------------------------------------------------------

void ParticleSystem::destroy( void )
{
    // Delete the particle system.
    delete Instance;
    Instance = NULL;
}

//------------------------------------------------------------------------------

ParticleSystem::ParticleSystem() :
                    mActiveParticleCount(0),
                    mAllocatedParticleCount(0)
{
}

//------------------------------------------------------------------------------

ParticleSystem::~ParticleSystem()
{
    mActiveParticleCount = 0;
    mAllocatedParticleCount = 0;
}

//------------------------------------------------------------------------------

void ParticleSystem::adjustActiveParticleCount( const S32 delta )
{
    if ( delta >= 0 )
    {
        mActiveParticleCount += (U32)delta;
        return;
    }

    const U32 absDelta = (U32)(-delta);
    mActiveParticleCount = (absDelta < mActiveParticleCount) ? (mActiveParticleCount - absDelta) : 0;
}

//------------------------------------------------------------------------------

void ParticleSystem::adjustAllocatedParticleCount( const S32 delta )
{
    if ( delta >= 0 )
    {
        mAllocatedParticleCount += (U32)delta;
        return;
    }

    const U32 absDelta = (U32)(-delta);
    mAllocatedParticleCount = (absDelta < mAllocatedParticleCount) ? (mAllocatedParticleCount - absDelta) : 0;
}

