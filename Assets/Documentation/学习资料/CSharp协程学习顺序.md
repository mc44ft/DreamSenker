# C# 协程学习顺序

## 目标

这份资料面向面试准备，不是只学 Unity 用法。

最终要能说清楚：

```text
C# 的 yield 会被编译器改写成 IEnumerator 状态机；
Unity 的 Coroutine 是 Unity 主线程调度这个 IEnumerator；
Unity 根据 yield return 出来的对象决定什么时候继续执行。
```

## 学习顺序

### 1. 先学 C# yield 和 IEnumerator

先理解协程底层，不要一上来只看 Unity 的 `StartCoroutine`。

资料：

- Microsoft yield 官方文档  
  https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/yield
- Microsoft C# Iterators  
  https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/iterators

重点：

- `yield return`
- `IEnumerator`
- `MoveNext()`
- `Current`
- 编译器生成状态机
- 局部变量如何被保存

面试回答核心：

```text
yield return 不是魔法，本质是编译器生成了一个实现 IEnumerator 的状态机对象。
每次调用 MoveNext()，状态机从上次暂停的位置继续执行。
```

### 2. 再学 Unity 协程调度

C# 只负责生成状态机，Unity 负责调度这个状态机。

资料：

- Unity Manual: Coroutines  
  https://docs.unity.cn/Manual/Coroutines.html
- Unity API: StartCoroutine  
  https://docs.unity.cn/2023.3/Documentation/ScriptReference/MonoBehaviour.StartCoroutine.html
- Unity Manual: Execution Order  
  https://docs.unity.cn/Manual/ExecutionOrder.html

重点：

- 协程不是线程
- Unity 协程默认运行在主线程
- `StartCoroutine` 不会阻塞调用者
- `yield return null` 表示下一帧继续
- `yield return new WaitForSeconds(x)` 表示等待时间后继续
- `yield return new WaitForFixedUpdate()` 表示等到下一次 FixedUpdate 阶段
- `yield return new WaitForEndOfFrame()` 表示等到当前帧渲染末尾

面试回答核心：

```text
Unity 拿到 IEnumerator 后，会在合适的 PlayerLoop 阶段继续调用 MoveNext()。
yield return 的对象决定 Unity 什么时候恢复这个协程。
```

### 3. 看状态机反编译思路

这一步是为了应对面试追问。

资料：

- Raymond Chen: The implementation of iterators in C#  
  https://devblogs.microsoft.com/oldnewthing/20080812-00/?p=21273
- Microsoft Magazine: Custom Iterators with Yield  
  https://learn.microsoft.com/en-us/archive/msdn-magazine/2017/june/essential-net-custom-iterators-with-yield

重点：

- `yield` 是语法糖
- 编译器生成隐藏类
- 隐藏类里通常有 `state` 字段
- 隐藏类里通常有 `current` 字段
- `MoveNext()` 内部通常是 `switch(state)`
- 局部变量会提升成状态机字段

可以这样理解：

```csharp
IEnumerator Foo()
{
    Debug.Log("A");
    yield return null;
    Debug.Log("B");
}
```

会被改写成类似：

```csharp
class FooStateMachine : IEnumerator
{
    private int _state;
    private object _current;

    public object Current => _current;

    public bool MoveNext()
    {
        switch (_state)
        {
            case 0:
                Debug.Log("A");
                _current = null;
                _state = 1;
                return true;

            case 1:
                Debug.Log("B");
                _state = -1;
                return false;
        }

        return false;
    }
}
```

### 4. 看 Unity 实战用法

这一步补常见写法，不要停留在“等几秒”。

资料：

- Unity Learn: Using Coroutines  
  https://learn.unity.com/tutorial/using-coroutines
- Unity Learn: Coroutines  
  https://learn.unity.com/tutorial/coroutines

重点：

- 分帧执行长逻辑
- 延迟执行
- 等待动画、特效、过场
- 等待条件
- 串联多个步骤
- 什么时候用协程，什么时候用 `Update`

常见判断：

```text
每帧都要持续检查的逻辑，用 Update 更直接。
有明确流程顺序、等待、延迟、分段执行的逻辑，用协程更清晰。
```

### 5. 最后看性能和坑

资料：

- Unity GC 最佳实践  
  https://docs.unity.cn/Manual/performance-garbage-collection-best-practices.html
- Coroutines and WaitForSeconds  
  https://giannisakritidis.com/blog/Coroutines-and-WaitForSeconds/

重点：

- `new WaitForSeconds()` 会产生对象分配
- 高频协程里反复 new 等待对象可能增加 GC 压力
- `yield return null` 通常不产生额外等待对象
- 协程数量过多会增加调度成本
- `StopCoroutine` 要注意传入方式
- `GameObject` 销毁时协程会停止
- `MonoBehaviour.enabled = false` 不一定停止协程
- `GameObject.SetActive(false)` 会停止协程

## 面试常见问题

### 协程是不是线程？

不是。

Unity 协程默认在主线程执行。它不会帮你并行计算，只是把逻辑拆成多段，在不同帧继续执行。

### `yield return null` 和 `yield return 0` 有什么区别？

Unity 里通常都会下一帧继续。

但应该写：

```csharp
yield return null;
```

不要写：

```csharp
yield return 0;
```

原因：

- `null` 语义明确
- `0` 会装箱成 `object`
- Unity 不把 `0` 当成特殊等待指令
- `0` 能跑但写法很烂

### 协程和 Update 有什么区别？

`Update` 是每帧主动被 Unity 调用。

协程是 Unity 按 `yield return` 的结果决定什么时候继续调用 `MoveNext()`。

适合协程的逻辑：

```text
等待几秒
等待一帧
等待动画结束
等待条件满足
分步骤执行流程
```

适合 `Update` 的逻辑：

```text
玩家输入
持续移动
持续检测
每帧刷新
```

### 协程和 async/await 有什么区别？

协程是 Unity 基于 `IEnumerator` 的调度机制。

`async/await` 是 C# 基于 `Task` / 状态机的异步机制。

简单区分：

```text
协程：更适合 Unity 主线程里的帧流程、动画、等待。
async/await：更适合 IO、网络、文件、真正异步任务。
```

### StopCoroutine 有什么坑？

不要混用启动和停止方式。

例如：

```csharp
Coroutine routine = StartCoroutine(Foo());
StopCoroutine(routine);
```

或者：

```csharp
StartCoroutine(nameof(Foo));
StopCoroutine(nameof(Foo));
```

不要启动时用 `IEnumerator`，停止时用字符串，容易停不到你以为的那个。

## 最小复习答案

面试被问“C# 协程底层是什么”，可以这样答：

```text
C# 的 yield return 会被编译器改写成一个实现 IEnumerator 的状态机类。
这个状态机保存当前执行位置、Current 和局部变量。
每次调用 MoveNext()，它会从上一次 yield 暂停的位置继续执行。

Unity 的 Coroutine 不是线程。
StartCoroutine 拿到 IEnumerator 后，由 Unity 在主线程 PlayerLoop 中调度。
yield return null、WaitForSeconds、WaitForFixedUpdate 等对象会告诉 Unity 什么时候再次调用 MoveNext()。
```
