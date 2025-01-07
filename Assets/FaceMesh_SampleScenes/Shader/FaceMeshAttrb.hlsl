#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

StructuredBuffer<float4> _Vertices  : register(t0);

void VertexTextured_float(uint ID , out float4 Out)
{
    Out = _Vertices[ID];
}
#endif