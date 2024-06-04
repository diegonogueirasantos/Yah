using AutoMapper;

namespace Yah.Hub.Common.Extensions
{
    public static class AutoMapperExtensions
    {
        public static IMappingExpression<TSource, TDest> IgnoreAllUnmapped<TSource, TDest>(this IMappingExpression<TSource, TDest> expression)
        {
            expression.ForAllMembers(opt => opt.Ignore());
            return expression;
        }

        public static void PassContextItem(this IMappingOperationOptions opt, ResolutionContext ctx)
        {
            foreach (var item in ctx.Items)
                opt.Items.Add(item);
        }
    }
}
