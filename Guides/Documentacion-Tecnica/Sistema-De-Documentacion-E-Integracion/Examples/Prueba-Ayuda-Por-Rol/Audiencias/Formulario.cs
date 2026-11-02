using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Audiencias;

// RN-DEMO-ASUNTO (Reglas/RN-DEMO-ASUNTO.md). El número vive acá; la letra, en Microtexto.resx.
public static class ReglaAsunto
{
    public const int Maximo = 80;
}

public class AvisoForm
{
    [Display(Name = "Asunto", Prompt = "Corte programado en la zona norte", Description = "Asunto.Pista")]
    [Required, StringLength(ReglaAsunto.Maximo)]
    public string Asunto { get; set; } = "";
}

public static class Microtexto
{
    public static readonly ResourceManager Rm = new("Audiencias.Microtexto", typeof(Microtexto).Assembly);
    // Interpolación: el límite sale de la regla; la plantilla la escribe una persona.
    public static string Texto(string clave, int limite) => string.Format(Rm.GetString(clave)!, limite);
}

// El campo como lo arma la vista: etiqueta, pista, InputText y ValidationMessage, con un error ya cargado.
public class CampoAsunto : ComponentBase
{
    [Parameter] public bool ConDescribedBy { get; set; }
    readonly AvisoForm modelo = new() { Asunto = "Corte programado en la zona norte por obras en la red; el servicio vuelve a la tarde, según el aviso ficticio" };
    EditContext ctx = null!;

    protected override void OnInitialized()
    {
        ctx = new EditContext(modelo);
        new ValidationMessageStore(ctx).Add(ctx.Field(nameof(AvisoForm.Asunto)), Microtexto.Texto("Asunto.Validacion", ReglaAsunto.Maximo));
        ctx.NotifyValidationStateChanged();
    }

    protected override void BuildRenderTree(RenderTreeBuilder b)
    {
        b.OpenComponent<CascadingValue<EditContext>>(0);
        b.AddAttribute(1, "Value", ctx);
        b.AddAttribute(2, "IsFixed", true);
        b.AddAttribute(3, "ChildContent", (RenderFragment)(c =>
        {
            c.OpenElement(0, "label"); c.AddAttribute(1, "for", "asunto"); c.AddContent(2, Microtexto.Rm.GetString("Asunto.Etiqueta")); c.CloseElement();
            c.OpenElement(3, "p"); c.AddAttribute(4, "id", "asunto-pista"); c.AddContent(5, Microtexto.Texto("Asunto.Pista", ReglaAsunto.Maximo)); c.CloseElement();
            c.OpenComponent<InputText>(6);
            c.AddAttribute(7, "id", "asunto");
            if (ConDescribedBy) c.AddAttribute(8, "aria-describedby", "asunto-pista asunto-error");
            c.AddAttribute(9, "Value", modelo.Asunto);
            c.AddAttribute(10, "ValueExpression", (Expression<Func<string>>)(() => modelo.Asunto));
            c.AddAttribute(11, "ValueChanged", EventCallback.Factory.Create<string>(this, v => modelo.Asunto = v));
            c.CloseComponent();
            c.OpenElement(12, "div"); c.AddAttribute(13, "id", "asunto-error");
            c.OpenComponent<ValidationMessage<string>>(14);
            c.AddAttribute(15, "For", (Expression<Func<string>>)(() => modelo.Asunto));
            c.CloseComponent();
            c.CloseElement();
        }));
        b.CloseComponent();
    }
}
