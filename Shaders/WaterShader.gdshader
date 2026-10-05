shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_lambert, specular_schlick_ggx;

uniform sampler2D flame_texture : filter_nearest;
uniform vec4 flame_tint : source_color = vec4(1.0, 0.3, 0.1, 1.0);
uniform float wave_speed : hint_range(0.0, 3.0) = 0.5;
uniform float wave_strength : hint_range(0.0, 0.1) = 0.02;
uniform float roughness_val : hint_range(0.0, 1.0) = 0.1;

uniform vec3 emission_color : source_color = vec3(1.0, 0.4, 0.1);
uniform float emission_energy : hint_range(0.0, 10.0) = 3.0;

void fragment() {
	vec2 uv = UV;
	uv.x += sin(TIME * wave_speed + UV.y * 10.0) * wave_strength;
	uv.y += cos(TIME * wave_speed + UV.x * 10.0) * wave_strength;

	vec4 tex_color = texture(flame_texture, uv);

	ALBEDO = tex_color.rgb * flame_tint.rgb;
	ALPHA = tex_color.a * flame_tint.a;
	ROUGHNESS = roughness_val;
	METALLIC = 0.0;

	EMISSION = emission_color * emission_energy;
}