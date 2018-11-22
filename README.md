# 作业批改助手

这款 WPF 小软件或许会对大学助教群体有所帮助吧……

![](example.png)

如果想要使用该软件，需要手动设置作业批语相关的 `.xml` 文件。上图所使用的模板如下：

```xml
<?xml version="1.0" encoding="utf-8" ?>
<task title="Lab 1" desc="这是整个项目的标题，通常只用来显示总分。">
    <task title="Task 1" desc="这是第一个题目。\n题目不一定总是有类似这样的描述信息。\n描述信息支持诸如\\n的换行符。">
        <subtask title="(a)" desc="这是第一小题。与题目不同的是，在批语中，小题前不会有空行。\n小题通常有分数，表示这道题目的总分。\n小题中通常包含多个扣分项。" score="30">
            <check title="第一个扣分项" score="-10"/>
            <check title="第二个扣分项" score="-10"/>
            <check title="第三个扣分项" score="-10"/>
            <check title="扣分项可以没有分数"/>
            <check title="也可以不显示内容，只需要结尾加上星号*"/>
        </subtask>
        <subtask title="(b)" desc="这是第二小题。" score="40">
            <check title="第一问回答错误" score="-10"/>
            <check title="第二问回答错误" score="-10"/>
            <group title="第三问回答有误" desc="这是一个选项分组，批语没有段前换行，而且后跟分号。\n其下通常有多个选项。任意一个勾选，都会将该分组的标题写入批语。\n因此分组中通常为加星号的扣分项。">
                <check title="扣分的第一个原因*" score="-10"/>
                <check title="扣分的第二个原因*" score="-5"/>
                <check title="扣分的第三个原因*" score="-5"/>
            </group>
        </subtask>
        <subtask title="(c)" desc="下面的项目的分数可以自行编辑，也可以用滚轮快速调节。">
            <mark title="整体印象分(0~10)" score="8" range="0,10"/>
        </subtask>
    </task>
</task>
```
