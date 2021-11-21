![castle](https://user-images.githubusercontent.com/8418700/141063129-d67ea604-312d-4b97-9d7f-5153dcced57f.png)

> In the field of software development, an interceptor pattern is a software design pattern that is used when software systems or frameworks want to offer a way to change, or augment, their usual processing cycle.

This library works on top of [Castle Core](https://github.com/castleproject/Core).

## [Nuget](https://www.nuget.org/packages/Intercepted)

[![Open Source Love](https://badges.frapsoft.com/os/mit/mit.svg?v=102)](https://opensource.org/licenses/MIT)
![Nuget](https://img.shields.io/nuget/v/Intercepted)
![Nuget](https://img.shields.io/nuget/dt/Intercepted)

```
Install-Package Intercepted

dotnet add package Intercepted
```

<hr/>

## Preparation

Let me show you how to write an interceptor by an example.

#### The key point

You can **only** control properties and methods that are `virtual`. So if you want to take a property or method out of the cycle, do not define it as virtual.

I want to take over the behaviors (properties/methods) of the following class.

```cs
public class Person
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string FamilyName { get; set; }
    public virtual int Age { get; set; }

    public virtual int GetBirthDateYear()
    {
        return DateTime.Now.Year - Age;
    }

    public virtual string GetFullName()
    {
        return $"{Name} {FamilyName}";
    }

    public virtual double DividingAgeByZero()
    {
        var zero = 0;
        return Age / zero;
    }
}
```

I define all properties and methods as `virtual` because I want to have full control on them.

It is time to write a simple interceptor for the `Person` class. For this purpose you should inherit from `BaseInterceptor`.

```cs
using Castle.DynamicProxy;

public class PersonInterceptor : BaseInterceptor
{
    // Before calling the specified method.
    // OnEntry is inside a 'try' block.
    protected override void OnEntry(IInvocation invocation)
    {
        base.OnEntry(invocation);
        Console.WriteLine($"OnEntry - {invocation.Method.Name} - {invocation.ReturnValue ?? "{NOTHING_YET}"}");
    }

    // After calling the specified method.
    // OnSuccess is inside a 'try' block.
    protected override void OnSuccess(IInvocation invocation)
    {
        base.OnSuccess(invocation);
        Console.WriteLine($"OnSuccess - {invocation.Method.Name} - {invocation.ReturnValue ?? "{NOTHING_YET}"}");
    }

    // Calls when an exception is raised.
    // OnException is inside a 'catch' block.
    protected override void OnException(IInvocation invocation, Exception ex)
    {
        base.OnException(invocation, ex);
        Console.WriteLine($"OnException - {invocation.Method.Name} - {ex.Message} - {invocation.ReturnValue ?? "{NOTHING_YET}"}");
    }

    // After finishing whole process.
    // OnExit is inside a 'finally' block.
    protected override void OnExit(IInvocation invocation)
    {

        base.OnExit(invocation);
        Console.WriteLine($"OnExit - {invocation.Method.Name} - {invocation.ReturnValue ?? "{NOTHING_YET}"}");
    }
}
```

As you can see, you have four methods to override. `invocation` variable give you all information about the intercepted property or method.

For introducing the `PersonInterceptor` to `Person` class we should add an attribute as following.

```cs
[Interceptor(typeof(PersonInterceptor))]
public class Person
{
    // ...
}    
```

Not bad to mention, you are able to define multiple `Interceptor` for a class. For example

```cs
[Interceptor(typeof(PersonInterceptor))]  // Order #1
[Interceptor(typeof(PeopleInterceptor))]  // Order #2
[Interceptor(typeof(SocietyInterceptor))] // Order #3
public class Person
{
    // ...
}    
```

The methods of each `Interceptor` are called in the order in which they are defined.

Finally, In order for creating the `Person` class instance with interceptor support, we should do as follows:

```cs
using Castle.DynamicProxy;

Person newPerson = New.Of<Person>();

newPerson.Id = 1;
newPerson.Name = "Hamed";
newPerson.FamilyName = "Fathi";
newPerson.Age = 33;

var birthDateYear = newPerson.GetBirthDateYear();
var fullName = newPerson.GetFullName();
var result = newPerson.DividingAgeByZero();
```

## Analyze it!

Let see what is happened!

```cs
OnEntry - set_Id - {NOTHING_YET}
OnSuccess - set_Id - {NOTHING_YET}
OnExit - set_Id - {NOTHING_YET}
OnEntry - set_Name - {NOTHING_YET}
OnSuccess - set_Name - {NOTHING_YET}
OnExit - set_Name - {NOTHING_YET}
OnEntry - set_FamilyName - {NOTHING_YET}
OnSuccess - set_FamilyName - {NOTHING_YET}
OnExit - set_FamilyName - {NOTHING_YET}
OnEntry - set_Age - {NOTHING_YET}
OnSuccess - set_Age - {NOTHING_YET}
OnExit - set_Age - {NOTHING_YET}
OnEntry - GetBirthDateYear - {NOTHING_YET}
OnEntry - get_Age - {NOTHING_YET}
OnSuccess - get_Age - 33
OnExit - get_Age - 33
OnSuccess - GetBirthDateYear - 1988
OnExit - GetBirthDateYear - 1988
OnEntry - GetFullName - {NOTHING_YET}
OnEntry - get_Name - {NOTHING_YET}
OnSuccess - get_Name - Hamed
OnExit - get_Name - Hamed
OnEntry - get_FamilyName - {NOTHING_YET}
OnSuccess - get_FamilyName - Fathi
OnExit - get_FamilyName - Fathi
OnSuccess - GetFullName - Hamed Fathi
OnExit - GetFullName - Hamed Fathi
OnEntry - DividingAgeByZero - {NOTHING_YET}
OnEntry - get_Age - {NOTHING_YET}
OnSuccess - get_Age - 33
OnExit - get_Age - 33
OnException - DividingAgeByZero - Attempted to divide by zero. - 0
OnExit - DividingAgeByZero - 0
```

Properties in C# are mutator methods. They start with `get_X` and `set_X`. So as you can see, `PersonInterceptor` works on them too.

For `DividingAgeByZero`, As we expected, we faced an exception. In this situation the library sets default value of return type for the value automatically (same as `default(ReturnType)`). For example `DividingAgeByZero` returns a `double` so the library sets `0` on `OnException` method.

## Manipulate It!

You are able to change and manipulate intercepted method anytime. To achieve the goal you should play with `IInvocation invocation` variable inside `OnX` methods.

For example we can change default value of `OnException`.

```cs
public class PersonInterceptor : BaseInterceptor
{
    // ...
    protected override void OnException(IInvocation invocation, Exception ex)
    {
        base.OnException(invocation, ex);
        
        // HERE
        if (invocation.Method.Name == "DividingAgeByZero")
        {
            // It is 'double' type.
            invocation.ReturnValue = -1.0;
        }

        Console.WriteLine($"OnException - {invocation.Method.Name} - {ex.Message} - {invocation.ReturnValue ?? "{NOTHING_YET}"}");
    }
    // ...
}
```

and the final result changes to

```
OnEntry - DividingAgeByZero - {NOTHING_YET}
OnEntry - get_Age - {NOTHING_YET}
OnSuccess - get_Age - 33
OnExit - get_Age - 33
OnException - DividingAgeByZero - Attempted to divide by zero. - -1
OnExit - DividingAgeByZero - -1
```
