#ifndef PROCEDURAL_LIB
#define PROCEDURAL_LIB

#include "../ShaderLib/noiseSimplex.cginc"

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
    
    float humidity;
    if (humidity_factor == 0)
        humidity = height < 0 ? 1 : 0;
    else
        humidity = pow(humidity_factor, clamp(height * 2, 0, 1));
    if (!has_ocean)
        humidity = 0;
    return humidity;
}


#endif