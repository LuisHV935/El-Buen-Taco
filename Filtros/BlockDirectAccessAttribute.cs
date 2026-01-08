using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace El_Buen_Taco.Filtros
{
    public class BlockDirectAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Método 1: Usar Headers["Referer"]
            var referer = context.HttpContext.Request.Headers["Referer"].ToString();

            // Método 2: Obtener desde HttpContext.Items si está disponible
            if (string.IsNullOrEmpty(referer))
            {
                // Si no hay referer, es acceso directo
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // Verificar que venga del mismo dominio
            var currentUrl = $"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}";

            if (!referer.StartsWith(currentUrl))
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }
        }
    }
}
