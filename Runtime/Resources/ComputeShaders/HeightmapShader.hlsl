float CalculateHeightValue(float3 vertex, float factor);
float CalculateNoise(float3 vertex);


float CalculateHeightValue(float3 vertex, float factor) {
    float calculated_height = CalculateNoise(factor * vertex);
    calculated_height += CalculateNoise(factor * vertex * 2) / 2;
    calculated_height += CalculateNoise(factor * vertex * 5) / 5;
	
    return calculated_height;
}

float CalculateNoise(float3 vertex) {
	float noise_value = sin(vertex.x) * cos(vertex.y);
	noise_value += sin(vertex.y) * cos(vertex.z);
	noise_value += sin(vertex.z) * cos(vertex.x);
	noise_value += cos(vertex.x) * sin(vertex.y);
	noise_value += cos(vertex.y) * sin(vertex.z);
	noise_value += cos(vertex.z) * sin(vertex.x);
	noise_value /= 6;
	return noise_value;
}