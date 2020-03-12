float3 HUEtoRGB(in float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}

float Epsilon = 1e-10;

float3 RGBtoHCV(in float3 RGB)
{
    // Based on work by Sam Hocevar and Emil Persson
    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
    return float3(H, C, Q.x);
}

float3 HSVtoRGB(in float3 HSV)
{
    float3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

float3 RGBtoHSV(in float3 RGB)
{
    float3 HCV = RGBtoHCV(RGB);
    float S = HCV.y / (HCV.z + Epsilon);
    return float3(HCV.x, S, HCV.z);
}

float4 RGBtoYUV(float4 rgba) {
    float4 yuva;
    yuva.r = rgba.r * 0.2126 + 0.7152 * rgba.g + 0.0722 * rgba.b;
    yuva.g = (rgba.b - yuva.r) / 1.8556;
    yuva.b = (rgba.r - yuva.r) / 1.5748;
    yuva.a = rgba.a;
    
    // Adjust to work on GPU
    yuva.gb += 0.5;
    
    return yuva;
}

float4 YUVtoRGB(float4 yuva) {
    yuva.gb -= 0.5;
    return float4(
        yuva.r * 1 + yuva.g * 0 + yuva.b * 1.5748,
        yuva.r * 1 + yuva.g * -0.187324 + yuva.b * -0.468124,
        yuva.r * 1 + yuva.g * 1.8556 + yuva.b * 0,
        yuva.a);
}

float HSVtoWarmth(float3 HSV) {
    return sin(HSV.r);
}


uniform sampler2D _CameraDepthTexture;


float Outline(float2 texCoord, float2 screenSize, float depthMul = 0.5)
{
    // screenSize = _CameraDepthTexture_TexelSize.xy;
    float mdepth = LinearEyeDepth(tex2D(_CameraDepthTexture, texCoord).r);
    float2 offset = float2(0.5/screenSize.x, 0.5/screenSize.y); // _DepthMix controls the thickness of the outline
    float diff = 0.0;

    diff += abs(mdepth - LinearEyeDepth(tex2D(_CameraDepthTexture, texCoord + float2(offset.x, 0)).r));
    diff += abs(mdepth - LinearEyeDepth(tex2D(_CameraDepthTexture, texCoord + float2(0, offset.y)).r));

    diff *= depthMul;

    return smoothstep(1, diff, 0);
}