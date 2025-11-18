#ifndef ALDURISPUZZLE
#define ALDURISPUZZLE

#include "_Random.cginc"

static const float SQRT2 = 1.4142135;
static const float SQRT2_2 = 0.70710678;

uniform float2 _PuzzleSize;
uniform float4 _PuzzleSeed;

struct ToothData
{
    float pos;
    float size;
    float flat;
    int dir;
};

struct PieceData
{
    ToothData leftTooth;
    ToothData rightTooth;
    ToothData topTooth;
    ToothData bottomTooth;
};

ToothData getToothDataFor(int x, int y, int horiz)
{
    ToothData o;
    // Scalars drawn by random number generator
    o.pos = random(float3(0.2347 * x, 2.5487 * y, 4.1219 * horiz) + _PuzzleSeed.xyz);
    o.size = random(float3(4.8514 * x, 0.5573 * y, 1.2378 * horiz) + _PuzzleSeed.yzw);
    o.flat = random(float3(3.7742 * x, 1.0579 * y, 2.2018 * horiz) + _PuzzleSeed.zwx);
    o.dir = (random(float3(7.5384 * x, 3.5627 * y, 6.5849 * horiz) + _PuzzleSeed.xwy) > 0.5) * 2 - 1;
    return o;
}

PieceData getDataFor(int x, int y)
{
    PieceData d;
    d.leftTooth = getToothDataFor(x - 1, y, 1);
    d.rightTooth = getToothDataFor(x, y, 1);
    d.topTooth = getToothDataFor(x, y, 0);
    d.bottomTooth = getToothDataFor(x, y - 1, 0);
    return d;
}


uint insideToothHoriz(uint doGrab, ToothData tooth, float2 uv, int farSide)
{
    float flat = lerp(1, 1.5, tooth.flat);
    float2 checkPos = float2((uv.x - farSide) * tooth.dir * flat, uv.y);
    float scale = lerp(1.0 / 4.0, 1.0 / 12.0, tooth.size) * 0.5; // 0.5 to convert to radius
    float baseY = lerp(0.3125, 0.6875, tooth.pos);

    uint intersecting = checkPos.x > 0;

    float cBigCheckStartX = (SQRT2_2 + 1.0) * (0.75 * scale);
    if (checkPos.x > cBigCheckStartX)
    {
        // Big round tip
        float cBigX = cBigCheckStartX + SQRT2_2 * scale;

        intersecting &= distance(checkPos, float2(cBigX, baseY)) < scale;
    }
    else
    {
        // Cutoff on sides
        float cSmallRad = scale * 0.75;
        float cSmallOffsetY = SQRT2_2 * scale + SQRT2_2 * cSmallRad;
        float2 smallOffsetUV = float2(checkPos.x, checkPos.y - baseY);

        intersecting &= abs(smallOffsetUV.y) < cSmallOffsetY;
        intersecting &= distance(smallOffsetUV, float2(cSmallRad, cSmallOffsetY)) > cSmallRad;
        intersecting &= distance(smallOffsetUV, float2(cSmallRad, -cSmallOffsetY)) > cSmallRad;
    }

    return doGrab != intersecting;
}

inline uint insideToothVerti(uint doGrab, ToothData tooth, float2 uv, int farSide)
{
    return insideToothHoriz(doGrab, tooth, float2(uv.y, uv.x), farSide);
}

#endif