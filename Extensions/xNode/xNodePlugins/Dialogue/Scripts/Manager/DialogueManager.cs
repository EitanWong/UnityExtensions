using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dialogue.Scripts.Manager
{
    [DisallowMultipleComponent]
    public class DialogueManager : MonoBehaviour
    {
        #region Singleton

        private static DialogueManager _instance = null;

        public static DialogueManager Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType(typeof(DialogueManager)) as DialogueManager;

                    if (!_instance)
                    {
                        var obj = new GameObject(nameof(DialogueManager));
                        _instance = obj.AddComponent<DialogueManager>();
                    }
                    else
                    {
                        _instance.gameObject.name = nameof(DialogueManager);
                    }
                }

                return _instance;
            }
        }

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(_instance.gameObject);
                OnInit();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Field

        [SerializeField] private DialogueGraph currentGraph;

        #endregion

        #region Event

        public delegate void DialogueEvent(DialogueInfo info);

        // ReSharper disable once InconsistentNaming
        public DialogueEvent onDialoguePlayEvent;

        // ReSharper disable once InconsistentNaming
        public DialogueEvent onMakeChooseEvent;

        #endregion

        #region Behaviour

        void OnInit()
        {
        }

        private void Start()
        {
            if (currentGraph)
                PlayDialogue(currentGraph);
        }

        #endregion

        #region Method

        /// <summary>
        /// 设置当前对话
        /// </summary>
        /// <param name="graph">对话图</param>
        public void PlayDialogue(DialogueGraph graph)
        {
            this.currentGraph = graph;
            this.currentGraph.Restart();
            onDialoguePlayEvent?.Invoke(GetCurrentInfo());
        }

        /// <summary>
        /// 选择选项
        /// </summary>
        /// <param name="i"></param>
        public void MakeOption(int i)
        {
            currentGraph.current.ChooseOption(i);
            onMakeChooseEvent?.Invoke(GetCurrentInfo());
        }

        public void Next()
        {
            if (currentGraph.current.options.Count > 0) return;
            MakeOption(0);
        }

        /// <summary>
        /// 获取当前选项信息
        /// </summary>
        /// <returns></returns>
        public DialogueInfo GetCurrentInfo()
        {
            if (!currentGraph) return default;
            var chat = currentGraph.current;
            if (!chat) return default;
            return DialogueInfo.Build(chat);
        }

        #endregion
    }
}