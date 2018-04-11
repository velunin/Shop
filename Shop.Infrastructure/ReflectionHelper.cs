using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rds.CaraBus.RequestResponse;

namespace Shop.Infrastructure
{
    internal static class ReflectionHelper
    {
        public static TResult CompileInstanceFactory<TResult>(Type type, params Type[] ctorSignature)
        {
            var genericArguments = typeof(TResult).GetGenericArguments();

            if (ctorSignature == null || !ctorSignature.Any())
            {
                if (genericArguments.Length != 1)
                {
                    throw new ArgumentException("Invalid number of arguments.");
                }

                return Expression.Lambda<TResult>(Expression.New(type)).Compile();
            }

            var constructorInfo = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public,
                null,
                CallingConventions.HasThis,
                ctorSignature,
                null);

            if (constructorInfo == null)
            {
                throw new ArgumentException($"Type {type} doesn't have ctor with passed arguments.");
            }

            var ctorArgumentTypes = constructorInfo.GetParameters().Select(p => p.ParameterType).ToList();
            var funcArgumentTypes = genericArguments.Take(genericArguments.Length - 1).ToList();

            if (funcArgumentTypes.Count != ctorArgumentTypes.Count)
            {
                throw new ArgumentException("Incorrect number of arguments");
            }

            var argumentPairs =
                funcArgumentTypes.Zip(ctorArgumentTypes, (s, d) => new { Source = s, Destination = d }).ToList();

            if (argumentPairs.All(a => a.Source == a.Destination))
            {
                var constructorParameters = constructorInfo
                    .GetParameters()
                    .Select(p => Expression.Parameter(p.ParameterType))
                    .ToList();

                var constructorCall = Expression.New(constructorInfo, constructorParameters);

                return Expression.Lambda<TResult>(constructorCall, constructorParameters).Compile();
            }

            var lambdaArguments = new List<ParameterExpression>();
            var blockVariables = new List<ParameterExpression>();
            var blockExpressions = new List<Expression>();
            var callArguments = new List<ParameterExpression>();

            foreach (var a in argumentPairs)
            {
                var sourceParameter = Expression.Parameter(a.Source);
                lambdaArguments.Add(sourceParameter);

                if (a.Source == a.Destination)
                {
                    callArguments.Add(sourceParameter);
                }
                else
                {
                    var destinationVariable = Expression.Variable(a.Destination);
                    var assignToDestination = Expression.Assign(
                        destinationVariable,
                        Expression.Convert(sourceParameter, a.Destination)
                    );

                    callArguments.Add(destinationVariable);
                    blockVariables.Add(destinationVariable);
                    blockExpressions.Add(assignToDestination);
                }
            }

            var callExpression = Expression.New(constructorInfo, callArguments);
            blockExpressions.Add(callExpression);

            var block = Expression.Block(blockVariables, blockExpressions);

            var lambdaExpression = Expression.Lambda<TResult>(block, lambdaArguments);

            return lambdaExpression.Compile();
        }

        public static Func<object, Exception, object> GetFaultMessageCreator(Type messageType)
        {
            var faultType = typeof(Fault<>).MakeGenericType(messageType);

            return CompileInstanceFactory<Func<object, Exception, object>>(
                faultType,
                messageType,
                typeof(Exception));
        }
    }
}