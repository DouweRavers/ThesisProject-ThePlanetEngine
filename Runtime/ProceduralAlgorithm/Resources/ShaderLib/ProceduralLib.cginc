#ifndef PROCEDURAL_LIB
#define PROCEDURAL_LIB

#include "./noiseSimplex.cginc"

float CalculateHeightValue(float3 vertex, float seed, float continent_scale, int octaves = 10){
    float height_value = 0;
    float divider = 0;
    for (float i = 1; i < octaves; i++)
    {
        height_value += snoise(continent_scale * i * vertex + seed) / i;
        divider += 1 / i;
    }
    return height_value / divider;	
}

float CalculateHeatValue(float3 vertex, float height, float solar_heat, float height_cooling, bool has_ocean){
    float locational_heat = 1 - abs(vertex.y);
    float altitude_heat;
    if (has_ocean) altitude_heat = clamp(1 - height, 0,1);
    else altitude_heat = (1 - height) / 2;
    float heat = pow(locational_heat, 2 * (1 - solar_heat)) * pow(altitude_heat, height_cooling);
    return heat;
}

float CalculateHumidityValue(float height, float humidity_factor, bool has_ocean){
    if (!has_ocean) return 0;
    float humidity = clamp(sqrt((1 - height/ (humidity_factor+0.0001))), 0, 1);
    return humidity;
}

float CalculateDepthValue(float height_value)
{
    return abs(clamp(height_value, -1,0));
}

#endif