// Include file for GPU pseudorandom number generation

#ifndef ALDURISRANDOM
#define ALDURISRANDOM

inline uint hash(uint x)
{
    x += (x << 10u);
    x ^= (x >> 6u);
    x += (x << 3u);
    x ^= (x >> 11u);
    x += (x << 15u);
    return x;
}

inline uint hash(uint3 v)
{
    return hash(v.x ^ hash(v.y) ^ hash(v.z));
}

inline float random(float3 f)
{
    const uint mantissaMask = 0x007FFFFFu;
    const uint one = 0x3F800000u;
    
    uint h = hash(asuint(f));
    h &= mantissaMask;
    h |= one;
    
    float r2 = asfloat(h);
    return r2 - 1.0;
}

#endif
