Shader "Hidden/Custom/VHS"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float _Blend;

			
    

    float4 float4(float x,float y,float z,float w){return float4(x,y,z,w);}
    float4 float4(float x){return float4(x,x,x,x);}
    float4 float4(float2 x,float2 y){return float4(float2(x.x,x.y),float2(y.x,y.y));}
    float4 float4(float3 x,float y){return float4(float3(x.x,x.y,x.z),y);}


    float3 float3(float x,float y,float z){return float3(x,y,z);}
    float3 float3(float x){return float3(x,x,x);}
    float3 float3(float2 x,float y){return float3(float2(x.x,x.y),y);}

    float2 float2(float x,float y){return float2(x,y);}
    float2 float2(float x){return float2(x,x);}

    float float(float x){return float(x);}
    
    

	struct VertexInput {
    float4 vertex : POSITION;
	float2 uv:TEXCOORD0;
    float4 tangent : TANGENT;
    float3 normal : NORMAL;
	//VertexInput
	};
	struct VertexOutput {
	float4 pos : SV_POSITION;
	float2 uv:TEXCOORD0;
	//VertexOutput
	};
	sampler2D _MainTex; 

	
	VertexOutput vert (VertexInput v)
	{
	VertexOutput o;
	o.pos = UnityObjectToClipPos (v.vertex);
	o.uv = v.uv;
	//VertexFactory
	return o;
	}
    
    
/*---- Most code is by fmodusmogulus -- Feel free to use :3
except pseudogaussian blur is based on CeeJayDK https://www.shadertoy.com/view/Mtl3Rj (thank you <3)
----*/

float3 sharpen(in sampler2D video, in float2 uv, in float strength) {
    float3 vid = float3(tex2D(video, uv));
    vid += float3(tex2D(video, uv + strength));
    vid -= float3(tex2D(video, uv - strength));
    vid += float3(tex2D(video, float2(uv.x + strength, uv.y)));
    vid -= float3(tex2D(video, float2(uv.x, uv.y + strength)));
        
    return vid;
}

//---------------------------- BLUR -----------------------------------
float SCurve (float x) {

		x = x * 2.0 - 1.0;
		return -x * abs(x) * 0.5 + x + 0.5;

}

float4 BlurH (sampler2D source, float2 size, float2 uv, float radius) {

	if (radius >= 1.0)
	{
		float4 A = float4(0.0); 
		float4 C = float4(0.0); 

		float width = 1.0 / size.x;

		float divisor = 0.0; 
        float weight = 0.0;
        
        float radiusMultiplier = 1.0 / radius;
        
 		for (float x = -radius; x <= radius; x++)
		{
			A = tex2D(source, uv + float2(x * width, 0.0));
            
            	weight = SCurve(1.0 - (abs(x) * radiusMultiplier)); 
            
            	C += A * weight; 
            
			divisor += weight; 
		}

		return float4(C.r / divisor, C.g / divisor, C.b / divisor, 1.0);
	}

	return tex2D(source, uv);
}
//---------------------------- * -----------------------------------
float3 rgb2hsv(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

// All components are in the range [0…1], including hue.
float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float3 stripeArtifact(in sampler2D video, in float2 uv,  in int stripes)
{
    //By fmodusmogulus
    float3 vid = float3(sharpen(_MainTex, uv, 0.000));
    for (int i = 0; i < stripes; i++) { 
        
        float cycletime = fmod(floor( abs( _Time.y + ((uv.x+0.0)/1.1) ) * 10.0 ), 10.0 )  / 10.0;
        
        uv = float2(clamp(uv.x + float(i)/1 + cycletime * uv.x  * 0.1 , 0.0, 1.0), uv.y);
        float2 uv_edge = float2(clamp(uv.x + float(i)/1 + cycletime * uv.x * 0.1 - 0.001 , 0.0, 1.0), uv.y);
        //float3 value = clamp(float3(tex2D(_MainTex, uv)), 0.8, 1.0) - 0.8;
        float3 value = float3(tex2D(_MainTex, uv));
        value = float3(tex2D(_MainTex, uv)) - float3(tex2D(_MainTex, uv_edge)) * 200.0;
        value = rgb2hsv(value);
        value = float3(value.x, 0.0, value.z * 20.0);
        //value.z = sin(value.z * 0.2) * 2.0;
        //value.z = clamp(vid.x, 0.9, 1.0) * 100.0 - (0.9*100.0);
        value.z = exp(clamp(value.z * -1.0 + 1.0, 0.95, 1.0) * 40.0 - (0.95*40.0));
        value.z = value.z * sin(_Time.y / 2.0) * 3.0;
        value = hsv2rgb(value);
        
        if (i % 2 == 0) {
            vid += (value / float(stripes));
        }
        
        else {
            vid -= (value / float(stripes));
        }
    }
    
    return clamp(vid, 0.0, 1.0);
}


//Color Dodge
float3 colorDodge (float3 target, float3 blend){
    float3 temp;
    temp.x = (blend.x > 0.5) ? (1.0-(1.0-target.x)*(1.0-2.0*(blend.x-0.5))) : (target.x * (2.0*blend.x));
    temp.y = (blend.y > 0.5) ? (1.0-(1.0-target.y)*(1.0-2.0*(blend.y-0.5))) : (target.y * (2.0*blend.y));
    temp.z = (blend.z > 0.5) ? (1.0-(1.0-target.z)*(1.0-2.0*(blend.z-0.5))) : (target.z * (2.0*blend.z));
    return target + blend -0.5;
}






    
    
    float4 Frag(VaryingsDefault i) : SV_Target
    {
	

    float sharpenamount = 0.0009;
    float blurAmount = 70.0;
    float rgbShift = 0.002;
    
    float2 uv = vertex_output.uv/1;
    

    float3 col = 0.5 + 0.5*cos(_Time.y+uv.xyx+float3(0,2,4));
    float3 video;
    
    //Cheap interlacing
    if (iFrame % 10 == 0 ) {
        float3 video = float3(tex2D(_MainTex ,uv));
    }
    else {
        video = video;
    }
    /*
    float2 uv_cyan = float2(uv.x + 0.05, uv.y + 0.05);
    float2 uv_yellow = float2(uv.x - 0.05, uv.y + 0.05);
    video.x = sharpen(_MainTex, uv_cyan, sharpenamount).x;
    video.y = sharpen(_MainTex, uv, sharpenamount).y;
    video.z = sharpen(_MainTex, uv_yellow, sharpenamount).z;
    */
    video = sharpen(_MainTex, uv, sharpenamount);
    //if (uv.y < sin(_Time.y) && uv.y > sin(_Time.y+0.1)) { uv.x -= (uv.y*0.1); }
    if (uv.y > 0.9 + (sin(_Time.y*1000.0) * 0.01) ) { uv.x -= (uv.x*0.3); }
    float4 background = float4(stripeArtifact(_MainTex, uv, 100), 1.0);
    float4 foreground = float4(float3(video.x, video.y, video.z), 1.0);
    float4 stripedVideo = lerp(background, foreground, clamp(foreground.y, 0.6, 0.7) * 2.5);
    stripedVideo = float4(colorDodge(stripedVideo.xyz, foreground.xyz), 1.0);
    
    float2 uv_cyan = float2(uv.x + rgbShift, uv.y + rgbShift * 1.3);
    float2 uv_yellow = float2(uv.x - rgbShift, uv.y + rgbShift * 1.3);
    
    float4 blurredVideo = BlurH(_MainTex, 1, uv, blurAmount);
    blurredVideo.x = BlurH(_MainTex, 1, uv_cyan, blurAmount).x;
    blurredVideo.z = BlurH(_MainTex, 1, uv_yellow, blurAmount).z;
    
    float3 colChanHSV = rgb2hsv(blurredVideo.xyz);
    colChanHSV = float3(colChanHSV.x - 0.0, colChanHSV.y*1.2, colChanHSV.z);
    
    colChanHSV.y += sin(_Time.y + uv.y * 10.1) * cos(uv.x + _Time.y) * 0.1;
    colChanHSV.y += sin(uv.y * 700.0) * cos(uv.x + _Time.y) * 0.1;
    
    float3 valChanHSV = rgb2hsv(stripedVideo.xyz);
    float3 combinedHSV = float3(colChanHSV.x, colChanHSV.y, valChanHSV.z);
    float3 combinedRGB = hsv2rgb(combinedHSV);

    return float4(combinedRGB, 1.0);
    //return float4(video, 1.0);

	}
	ENDHLSL
    
}
  
  
