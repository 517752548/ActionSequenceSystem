// Copyright: ZhongShan KPP Technology Co
// Date: 2018-02-09
// Time: 14:54
// Author: Karsion

using System;
using System.Collections.Generic;
using UnityEngine;

//������������
public class ActionSequence
{
#if UNITY_EDITOR
    public static void GetObjectPoolInfo(out int countActive, out int countAll)
    {
        countActive = opSequences.countActive;
        countAll = opSequences.countAll;
    }
#endif

    internal static readonly ObjectPool<ActionSequence> opSequences = new ObjectPool<ActionSequence>(64);
    //�ڵ��б�Ĭ�ϰ�����������Ϊ8
    public readonly List<ActionNode> nodes = new List<ActionNode>(8);

    //Ŀ�������������ٵ�ʱ�򣬱���������Ҳ��Ӧ����
    public Component id { get; private set; }
    //��ǰִ�еĽڵ�����
    private int nCurIndex = 0;
    //��Ҫѭ���Ĵ���
    public int nLoopTime { get; private set; }
    //�Ѿ����еĴ���
    public int nRunLoopTime { get; private set; }
    //�Ƿ��Ѿ�������
    public bool isFinshed { get; private set; }

    //����ֹͣ
    public void Stop()
    {
        id = null;
        isFinshed = true;
        nRunLoopTime = 0;
        nLoopTime = 0;
    }

    //����һ�����нڵ�
    public ActionSequence Interval(float interval)
    {
        nodes.Add(ActionNodeInterval.Get(interval));
        return this;
    }

    //����һ�������ڵ�
    public ActionSequence Action(Action action)
    {
        nodes.Add(ActionNodeAction.Get(action));
        return this;
    }

    //����һ����ѭ�������Ķ����ڵ�
    public ActionSequence Action(Action<int> action)
    {
        ActionNodeAction actionNodeAction = ActionNodeAction.Get(action);
        actionNodeAction.UpdateLoopTime(nRunLoopTime);
        nodes.Add(actionNodeAction);
        return this;
    }

    //����һ�������ڵ�
    public ActionSequence Condition(Func<bool> condition)
    {
        nodes.Add(ActionNodeCondition.Get(condition));
        return this;
    }

    //����ѭ��
    public ActionSequence Loop(int loopTime = -1)
    {
        if (loopTime > 0)
        {
            nLoopTime = loopTime - 1;
            return this;
        }

        nLoopTime = loopTime;
        return this;
    }

    //��������
    private ActionSequence Start(Component id)
    {
        this.id = id;
        nCurIndex = 0;
        isFinshed = false;
        nRunLoopTime = 0;
        return this;
    }

    //���и���
    public void Update(float deltaTime)
    {
        //�������Ѿ�Stop��
        if (isFinshed)
        {
            return;
        }

        //����������id��������
        if (!id)
        {
            isFinshed = true;
            return;
        }

        //���������½ڵ�
        if (nodes[nCurIndex].Update(deltaTime))
        {
            nCurIndex++;
            if (nCurIndex >= nodes.Count)
            {
                //����ѭ���Ľڵ�
                if (nLoopTime < 0)
                {
                    Restart();
                    return;
                }

                //ѭ���Ľڵ���Ҫ�������������д���++
                if (nLoopTime > nRunLoopTime)
                {
                    Restart();
                    return;
                }

                //���д���>=ѭ�������ˣ���ֹͣ
                isFinshed = true;
                return;
            }

            nodes[nCurIndex].UpdateLoopTime(nRunLoopTime);
        }
    }

    //�������У����������еĽڵ�
    internal void Release()
    {
        nRunLoopTime = 0;
        opSequences.Release(this);
        nodes.ForEach(node => node.Release());
        nodes.Clear();
    }

    //��������
    private void Restart()
    {
        nRunLoopTime++;
        nCurIndex = 0;
        nodes.ForEach(node => node.Restart());
    }

    internal static ActionSequence GetInstance(Component component)
    {
        return opSequences.Get().Start(component);
    }
}