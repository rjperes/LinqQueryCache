using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqQueryCache
{
    sealed class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        #region Private fields
        private int _hashCode;
        #endregion

        #region Hash code
        private void Visit(Expression expression)
        {
            if (expression == null)
            {
                return;
            }

            this._hashCode ^= (int)expression.NodeType ^ expression.Type.GetHashCode();

            switch (expression.NodeType)
            {
                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    this.VisitUnary((UnaryExpression)expression);
                    break;

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    this.VisitBinary((BinaryExpression)expression);
                    break;

                case ExpressionType.Call:
                    this.VisitMethodCall((MethodCallExpression)expression);
                    break;

                case ExpressionType.Conditional:
                    this.VisitConditional((ConditionalExpression)expression);
                    break;

                case ExpressionType.Constant:
                    this.VisitConstant((ConstantExpression)expression);
                    break;

                case ExpressionType.Invoke:
                    this.VisitInvocation((InvocationExpression)expression);
                    break;

                case ExpressionType.Lambda:
                    this.VisitLambda((LambdaExpression)expression);
                    break;

                case ExpressionType.ListInit:
                    this.VisitListInit((ListInitExpression)expression);
                    break;

                case ExpressionType.MemberAccess:
                    this.VisitMemberAccess((MemberExpression)expression);
                    break;

                case ExpressionType.MemberInit:
                    this.VisitMemberInit((MemberInitExpression)expression);
                    break;

                case ExpressionType.New:
                    this.VisitNew((NewExpression)expression);
                    break;

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    this.VisitNewArray((NewArrayExpression)expression);
                    break;

                case ExpressionType.Parameter:
                    this.VisitParameter((ParameterExpression)expression);
                    break;

                case ExpressionType.TypeIs:
                    this.VisitTypeIs((TypeBinaryExpression)expression);
                    break;

                default:
                    throw new ArgumentException($"Unhandled expression type: {expression.NodeType}");
            }
        }

        private void VisitUnary(UnaryExpression expression)
        {
            if (expression.Method != null)
            {
                this._hashCode ^= expression.Method.GetHashCode();
            }

            this.Visit(expression.Operand);
        }

        private void VisitBinary(BinaryExpression expression)
        {
            if (expression.Method != null)
            {
                this._hashCode ^= expression.Method.GetHashCode();
            }

            this.Visit(expression.Left);
            this.Visit(expression.Right);
            this.Visit(expression.Conversion!);
        }

        private void VisitMethodCall(MethodCallExpression expression)
        {
            this._hashCode ^= expression.Method.GetHashCode();

            this.Visit(expression.Object!);
            this.VisitExpressionList(expression.Arguments);
        }

        private void VisitConditional(ConditionalExpression expression)
        {
            this.Visit(expression.Test);
            this.Visit(expression.IfTrue);
            this.Visit(expression.IfFalse);
        }

        private void VisitConstant(ConstantExpression expression)
        {
            if (expression.Value != null)
            {
                this._hashCode ^= expression.Value.GetHashCode();
            }
        }

        private void VisitInvocation(InvocationExpression expression)
        {
            this.Visit(expression.Expression);
            this.VisitExpressionList(expression.Arguments);
        }

        private void VisitLambda(LambdaExpression expression)
        {
            if (expression.Name != null)
            {
                this._hashCode ^= expression.Name.GetHashCode();
            }

            this.Visit(expression.Body);
            this.VisitParameterList(expression.Parameters);
        }

        private void VisitListInit(ListInitExpression expression)
        {
            this.VisitNew(expression.NewExpression);
            this.VisitElementInitializerList(expression.Initializers);
        }

        private void VisitMemberAccess(MemberExpression expression)
        {
            this._hashCode ^= expression.Member.GetHashCode();
            this.Visit(expression.Expression!);
        }

        private void VisitMemberInit(MemberInitExpression expression)
        {
            this.Visit(expression.NewExpression);
            this.VisitBindingList(expression.Bindings);
        }

        private void VisitNew(NewExpression expression)
        {
            this._hashCode ^= expression.Constructor!.GetHashCode();

            this.VisitMemberList(expression.Members!);
            this.VisitExpressionList(expression.Arguments);
        }

        private void VisitNewArray(NewArrayExpression expression)
        {
            this.VisitExpressionList(expression.Expressions);
        }

        private void VisitParameter(ParameterExpression expression)
        {
            if (expression.Name != null)
            {
                this._hashCode ^= expression.Name.GetHashCode();
            }
        }

        private void VisitTypeIs(TypeBinaryExpression expression)
        {
            this._hashCode ^= expression.TypeOperand.GetHashCode();
            this.Visit(expression.Expression);
        }

        private void VisitBinding(MemberBinding binding)
        {
            this._hashCode ^= (int)binding.BindingType ^ binding.Member.GetHashCode();

            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    this.VisitMemberAssignment((MemberAssignment)binding);
                    break;

                case MemberBindingType.MemberBinding:
                    this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                    break;

                case MemberBindingType.ListBinding:
                    this.VisitMemberListBinding((MemberListBinding)binding);
                    break;

                default:
                    throw new ArgumentException("Unhandled binding type");
            }
        }

        private void VisitMemberAssignment(MemberAssignment assignment)
        {
            this.Visit(assignment.Expression);
        }

        private void VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            this.VisitBindingList(binding.Bindings);
        }

        private void VisitMemberListBinding(MemberListBinding binding)
        {
            this.VisitElementInitializerList(binding.Initializers);
        }

        private void VisitElementInitializer(ElementInit initializer)
        {
            this._hashCode ^= initializer.AddMethod.GetHashCode();

            this.VisitExpressionList(initializer.Arguments);
        }

        private void VisitExpressionList(ReadOnlyCollection<Expression> list)
        {
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    this.Visit(list[i]);
                }
            }
        }

        private void VisitParameterList(ReadOnlyCollection<ParameterExpression> list)
        {
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    this.Visit(list[i]);
                }
            }
        }

        private void VisitBindingList(ReadOnlyCollection<MemberBinding> list)
        {
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    this.VisitBinding(list[i]);
                }
            }
        }

        private void VisitElementInitializerList(ReadOnlyCollection<ElementInit> list)
        {
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    this.VisitElementInitializer(list[i]);
                }
            }
        }

        private void VisitMemberList(ReadOnlyCollection<MemberInfo> list)
        {
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    this._hashCode ^= list[i].GetHashCode();
                }
            }
        }
        #endregion

        #region Equality
        private bool Visit(Expression x, Expression y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if ((x == null) || (y == null))
            {
                return false;
            }

            if ((x.NodeType != y.NodeType) || (x.Type != y.Type))
            {
                return false;
            }

            switch (x.NodeType)
            {
                case ExpressionType.ArrayLength:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)x, (UnaryExpression)y);

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return this.VisitBinary((BinaryExpression)x, (BinaryExpression)y);

                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)x, (MethodCallExpression)y);

                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)x, (ConditionalExpression)y);

                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)x, (ConstantExpression)y);

                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)x, (InvocationExpression)y);

                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)x, (LambdaExpression)y);

                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)x, (ListInitExpression)y);

                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)x, (MemberExpression)y);

                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)x, (MemberInitExpression)y);

                case ExpressionType.New:
                    return this.VisitNew((NewExpression)x, (NewExpression)y);

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)x, (NewArrayExpression)y);

                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)x, (ParameterExpression)y);

                case ExpressionType.TypeIs:
                    return this.VisitTypeIs((TypeBinaryExpression)x, (TypeBinaryExpression)y);

                default:
                    throw new ArgumentException("Unhandled expression type");
            }
        }

        private bool VisitUnary(UnaryExpression x, UnaryExpression y)
        {
            return (x.Method == y.Method) &&
                   (this.Visit(x.Operand, y.Operand));
        }

        private bool VisitBinary(BinaryExpression x, BinaryExpression y)
        {
            return (x.Method == y.Method) &&
                   (this.Visit(x.Left, y.Left)) &&
                   (this.Visit(x.Right, y.Right)) &&
                   (this.Visit(x.Conversion!, y.Conversion!));
        }

        private bool VisitMethodCall(MethodCallExpression x, MethodCallExpression y)
        {
            return (x.Method == y.Method) &&
                   (this.Visit(x.Object!, y.Object!)) &&
                   (this.VisitExpressionList(x.Arguments, y.Arguments));
        }

        private bool VisitConditional(ConditionalExpression x, ConditionalExpression y)
        {
            return (this.Visit(x.Test, y.Test)) &&
                   (this.Visit(x.IfTrue, y.IfTrue)) &&
                   (this.Visit(x.IfFalse, y.IfFalse));
        }

        private bool VisitConstant(ConstantExpression x, ConstantExpression y)
        {
            return object.Equals(x.Value, y.Value);
        }

        private bool VisitInvocation(InvocationExpression x, InvocationExpression y)
        {
            return (this.Visit(x.Expression, y.Expression)) &&
                   (this.VisitExpressionList(x.Arguments, x.Arguments));
        }

        private bool VisitLambda(LambdaExpression x, LambdaExpression y)
        {
            return (this.Visit(x.Body, y.Body)) &&
                   (this.VisitParameterList(x.Parameters, y.Parameters));
        }

        private bool VisitListInit(ListInitExpression x, ListInitExpression y)
        {
            return ((this.VisitNew(x.NewExpression, y.NewExpression)) &&
                   (this.VisitElementInitializerList(x.Initializers, y.Initializers)));
        }

        private bool VisitMemberAccess(MemberExpression x, MemberExpression y)
        {
            return (x.Member == y.Member) &&
                   (this.Visit(x.Expression!, y.Expression!));
        }

        private bool VisitMemberInit(MemberInitExpression x, MemberInitExpression y)
        {
            return (this.Visit(x.NewExpression, y.NewExpression) &&
                   (this.VisitBindingList(x.Bindings, y.Bindings)));
        }

        private bool VisitNew(NewExpression x, NewExpression y)
        {
            return (x.Constructor == y.Constructor) &&
                   (this.VisitMemberList(x.Members!, y.Members!)) &&
                   (this.VisitExpressionList(x.Arguments, y.Arguments));
        }

        private bool VisitNewArray(NewArrayExpression x, NewArrayExpression y)
        {
            return this.VisitExpressionList(x.Expressions, y.Expressions);
        }

        private bool VisitParameter(ParameterExpression x, ParameterExpression y)
        {
            return (x.Type == y.Type) && (x.IsByRef == y.IsByRef);
        }

        private bool VisitTypeIs(TypeBinaryExpression x, TypeBinaryExpression y)
        {
            return (x.TypeOperand == y.TypeOperand) &&
                   (this.Visit(x.Expression, y.Expression));
        }

        private bool VisitBinding(MemberBinding x, MemberBinding y)
        {
            if ((x.BindingType != y.BindingType) || (x.Member != y.Member))
            {
                return false;
            }

            switch (x.BindingType)
            {
                case MemberBindingType.Assignment:
                    return this.VisitMemberAssignment((MemberAssignment)x, (MemberAssignment)y);

                case MemberBindingType.MemberBinding:
                    return this.VisitMemberMemberBinding((MemberMemberBinding)x, (MemberMemberBinding)y);

                case MemberBindingType.ListBinding:
                    return this.VisitMemberListBinding((MemberListBinding)x, (MemberListBinding)y);

                default:
                    throw new ArgumentException("Unhandled binding type");
            }
        }

        private bool VisitMemberAssignment(MemberAssignment x, MemberAssignment y)
        {
            return this.Visit(x.Expression, y.Expression);
        }

        private bool VisitMemberMemberBinding(MemberMemberBinding x, MemberMemberBinding y)
        {
            return this.VisitBindingList(x.Bindings, y.Bindings);
        }

        private bool VisitMemberListBinding(MemberListBinding x, MemberListBinding y)
        {
            return this.VisitElementInitializerList(x.Initializers, y.Initializers);
        }

        private bool VisitElementInitializer(ElementInit x, ElementInit y)
        {
            return (x.AddMethod == y.AddMethod) &&
                   (this.VisitExpressionList(x.Arguments, y.Arguments));
        }

        private bool VisitExpressionList(ReadOnlyCollection<Expression> x, ReadOnlyCollection<Expression> y)
        {
            if (x == y)
            {
                return true;
            }

            if ((x != null) && (y != null) && (x.Count == y.Count))
            {
                for (var i = 0; i < x.Count; i++)
                {
                    if (!this.Visit(x[i], y[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private bool VisitParameterList(ReadOnlyCollection<ParameterExpression> x, ReadOnlyCollection<ParameterExpression> y)
        {
            if (x == y)
            {
                return true;
            }

            if ((x != null) && (y != null) && (x.Count == y.Count))
            {
                for (var i = 0; i < x.Count; i++)
                {
                    if (!this.Visit(x[i], y[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private bool VisitBindingList(ReadOnlyCollection<MemberBinding> x, ReadOnlyCollection<MemberBinding> y)
        {
            if (x == y)
            {
                return true;
            }

            if ((x != null) && (y != null) && (x.Count == y.Count))
            {
                for (var i = 0; i < x.Count; i++)
                {
                    if (!this.VisitBinding(x[i], y[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private bool VisitElementInitializerList(ReadOnlyCollection<ElementInit> x, ReadOnlyCollection<ElementInit> y)
        {
            if (x == y)
            {
                return true;
            }

            if ((x != null) && (y != null) && (x.Count == y.Count))
            {
                for (var i = 0; i < x.Count; i++)
                {
                    if (!this.VisitElementInitializer(x[i], y[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private bool VisitMemberList(ReadOnlyCollection<MemberInfo> x, ReadOnlyCollection<MemberInfo> y)
        {
            if (x == y)
            {
                return true;
            }

            if ((x != null) && (y != null) && (x.Count == y.Count))
            {
                for (var i = 0; i < x.Count; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
        #endregion

        #region IEqualityComparer<Expression> Members
        public bool Equals(Expression? x, Expression? y)
        {
            return this.Visit(x!, y!);
        }

        public int GetHashCode(Expression expression)
        {
            this._hashCode = 0;

            this.Visit(expression);

            return this._hashCode;
        }
        #endregion
    }
}