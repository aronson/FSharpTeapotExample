#version 330 core

uniform mat4 matrix;
layout(location = 0) in vec3 aPosition;

void main(void)
{
    gl_Position = matrix * vec4(aPosition, 1.0);
}