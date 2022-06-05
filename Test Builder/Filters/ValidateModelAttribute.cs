
using Microsoft.AspNetCore.Mvc.Filters;
//using System.Net;

namespace ecommerce.Filters
{
    public class ValidateModelAttribute : Attribute, IActionFilter
    {
        
        public ValidateModelAttribute() { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //if (ModelState.IsValid)
            //{

            //}
            Console.WriteLine("OnActionExecuting");
        }

        //public Task<HttpResponseMessage> Ex
    }
}