using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(VHSRenderer), PostProcessEvent.AfterStack, "Custom/VHS")]
public sealed class VHS : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("VHS effect intensity.")]
    public FloatParameter blend = new FloatParameter { value = 0.5f };
}

public sealed class VHSRenderer : PostProcessEffectRenderer<VHS>
{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/VHS"));
        sheet.properties.SetFloat("_Blend", settings.blend);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}