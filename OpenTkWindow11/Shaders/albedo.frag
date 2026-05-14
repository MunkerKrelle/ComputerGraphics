#version 330 core
out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D albedo;
uniform vec3 tintColor;

void main()
{
    outputColor = texture(albedo, texCoord) * vec4(tintColor, 1.0);
}