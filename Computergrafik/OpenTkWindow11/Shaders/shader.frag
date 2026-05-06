#version 400
out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform float theta;
void main()
{

	//outputColor = texture(texture0,texCoord);
    vec4 dst  = texture(texture0, texCoord);
    vec2 texCoordOffset = texCoord- vec2(.5,.5);

    float xWithRot = cos(theta)*(texCoordOffset.x) - sin(theta)*(texCoordOffset.y);    
    float yWithRot= sin(theta)*(texCoordOffset.x) +cos(theta)*(texCoordOffset.y); 
    
    vec4 src = texture(texture1, vec2(xWithRot+.5,yWithRot+.5));
     float final_alpha = src.a + dst.a * (1.0 - src.a);
    outputColor = vec4(
        (src.rgb * src.a + dst.rgb * dst.a * (1.0 - src.a)) / final_alpha,
        final_alpha
    );
}