﻿// These shaders are not used by the game engine by default, because it actually uses built-in embedded shaders
// accessible from the BuiltInShaders static class. 

//This shader file servers as something to work off of.

#version 330 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec3 in_normal;
layout(location = 3) in vec4 color;

layout(location = 4) in vec2 particlePosition;
layout(location = 5) in vec4 particleColor;

out vec2 uv;
out vec4 vertexColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
   uv = texcoord;
   vertexColor = particleColor;
   
   gl_Position = projection * view * model * (vec4(position, 1.0) + vec4(particlePosition.x, particlePosition.y, 0, 0));
}