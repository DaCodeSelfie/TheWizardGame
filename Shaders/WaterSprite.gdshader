shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_lambert, specular_schlick_ggx;

uniform sampler2D water_texture : filter_nearest;
uniform vec4 water_tint : source_color = vec4(1.0, 1.0, 1.0, 0.8);
uniform float wave_speed : hint_range(0.0, 3.0) = 0.5;
uniform float wave_strength : hint_range(0.0, 0.1) = 0.02;
uniform float roughness_val : hint_range(0.0, 1.0) = 0.1;

uniform int frame_count = 5;
uniform float anim_speed = 6.0;

uniform vec2 tile_repeat = vec2(5.0, 5.0); // how many times the texture repeats across the mesh

void fragment() {
	// Repeat the UV across the surface based on tile_repeat
	vec2 tiled_uv = fract(UV * tile_repeat);

	// Ripple distortion, now applied to the tiled UV
	tiled_uv.x += sin(TIME * wave_speed + tiled_uv.y * 10.0) * wave_strength;
	tiled_uv.y += cos(TIME * wave_speed + tiled_uv.x * 10.0) * wave_strength;

	// Frame-cycling for the sprite sheet, applied within each tile's own 0-1 UV space
	int current_frame = int(mod(TIME * anim_speed, float(frame_count)));
	float frame_width = 1.0 / float(frame_count);
	tiled_uv.x = (tiled_uv.x * frame_width) + (frame_width * float(current_frame));

	vec4 tex_color = texture(water_texture, tiled_uv);

	ALBEDO = tex_color.rgb * water_tint.rgb;
	ALPHA = tex_color.a * water_tint.a;
	ROUGHNESS = roughness_val;
	METALLIC = 0.0;
}