using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;

public class AnimationSimulatorWindow : EditorWindow
{
    private int m_selectedAnimatorIndex = -1;
    private int m_selectedAnimationIndex = -1;
    private Animator m_selectedAnimator;
    private AnimationClip m_selectedClip;

    private VisualElement m_rightPane;
    private ListView m_animatorsListView;
    private ListView m_animationsListView;
    private Label m_labelInfo;

    private bool m_isPlaying = false;
    private bool m_isAnimating = false;
    private float m_editorTime = 0;
    private float m_currentAnimationSpeed = 1;
    private bool m_currentAnimationLoop = false;

    [MenuItem("Tools/AnimationSimulator")]
    public static void ShowEditor()
    {
        EditorWindow window = GetWindow<AnimationSimulatorWindow>();
        window.titleContent = new GUIContent("Animation Simulator");

        window.minSize = new Vector2(450, 200);
        window.maxSize = new Vector2(1920, 720);
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        EditorApplication.hierarchyChanged += RefreshUI;
        EditorSceneManager.activeSceneChangedInEditMode += EditorSceneManager_activeSceneChangedInEditMode;
    }

    private void EditorSceneManager_activeSceneChangedInEditMode(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
    {
        //Debug.Log("Hello i have changed");
        StopAnimating();
        m_animationsListView.ClearSelection();
        m_animationsListView.ClearSelection();
        m_rightPane.Clear();
        RefreshUI();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
        EditorApplication.hierarchyChanged -= RefreshUI;
        StopAnimating();
    }

    public void CreateGUI()
    {
        var allObjects = new List<Animator>();
        allObjects = Object.FindObjectsOfType<Animator>().ToList();

        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        rootVisualElement.Add(splitView);

        var splitViewLeft = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Vertical);
        splitView.Add(splitViewLeft);

        m_animatorsListView = new ListView();
        splitViewLeft.Add(m_animatorsListView);
        m_animationsListView = new ListView();
        splitViewLeft.Add(m_animationsListView);

        m_rightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
        splitView.Add(m_rightPane);

        m_animatorsListView.makeItem = () => new Label();
        m_animatorsListView.bindItem = (item, index) => { (item as Label).text = allObjects[index].name; };
        m_animatorsListView.itemsSource = allObjects;

        m_animatorsListView.onSelectionChange += OnAnimatorChange;

        m_animatorsListView.selectedIndex = m_selectedAnimatorIndex;
        m_animatorsListView.onSelectionChange += (items) => { m_selectedAnimatorIndex = m_animatorsListView.selectedIndex; };

        m_animationsListView.onSelectionChange += OnAnimationChange;
        m_animationsListView.selectedIndex = m_selectedAnimationIndex;
        m_animationsListView.onSelectionChange += (items) => { m_selectedAnimationIndex = m_animationsListView.selectedIndex; };
    }

    private void OnAnimatorChange(IEnumerable<object> selectedItems)
    {
        m_rightPane.Clear();
        m_currentAnimationLoop = false;
        m_currentAnimationSpeed = 1f;
        StopAnimating();
        //m_animListPane.Clear();
        m_animationsListView.ClearSelection();
        //m_animListPane.MarkDirtyRepaint();
        //Clear list view unity

        m_selectedAnimator = selectedItems.First() as Animator;
        if (m_selectedAnimator == null)
            return;

        if (m_selectedAnimator.transform.parent != null)
            Selection.activeGameObject = m_selectedAnimator.transform.parent.gameObject;
        else
            Selection.activeGameObject = m_selectedAnimator.gameObject;
        SceneView.FrameLastActiveSceneView();

        if (m_selectedAnimator.runtimeAnimatorController == null)
            return;

        var allclips = m_selectedAnimator.runtimeAnimatorController.animationClips;
        if (allclips.Length == 0)
            return;


        //m_animationsListView.Clear();
        //m_animationsListView.Refresh();
        m_animationsListView.itemsSource = null;
        m_animationsListView.makeItem = () => new Label();
        m_animationsListView.bindItem = (item, index) => { (item as Label).text = allclips[index].name; };
        m_animationsListView.itemsSource = allclips;
    }

    private void OnAnimationChange(IEnumerable<object> selectedAnims)
    {
        m_rightPane.Clear();

        m_selectedClip = selectedAnims.FirstOrDefault() as AnimationClip;
        if (m_selectedClip == null)
            return;

        GenerateRightPaneUI();
    }

    private void GenerateRightPaneUI()
    {
        var buttonBox = new Box();
        buttonBox.style.flexDirection = FlexDirection.Row;
        buttonBox.style.justifyContent = Justify.Center;

        var buttonBeginning = new Button();
        buttonBeginning.text = "<<";
        buttonBeginning.clicked += () =>
        {
            if (!m_isPlaying)
            {
                m_selectedClip.SampleAnimation(m_selectedAnimator.gameObject, 0);
                m_labelInfo.text = $"0,00 / {m_selectedClip.length.ToString("F2")}";
            }
        };

        var buttonPlay = new Button();
        buttonPlay.text = "Play";
        buttonPlay.clicked += () =>
        {
            if (!m_isPlaying)
            {
                AnimationMode.StartAnimationMode();
                EditorApplication.update -= OnEditorUpdate;
                EditorApplication.update += OnEditorUpdate;
                m_editorTime = Time.realtimeSinceStartup;
                m_isAnimating = true;
            }
        };

        var buttonPause = new Button();
        buttonPause.text = "Pause";
        buttonPause.clicked += () =>
        {
            StopAnimating();
        };

        var buttonEnding = new Button();
        buttonEnding.text = ">>";
        buttonEnding.clicked += () =>
        {
            if (!m_isPlaying)
            {
                m_selectedClip.SampleAnimation(m_selectedAnimator.gameObject, m_selectedClip.length);
                m_labelInfo.text = $"{m_selectedClip.length.ToString("F2")} / {m_selectedClip.length.ToString("F2")}";
            }
        };

        m_labelInfo = new Label();
        m_labelInfo.text = $"0,00 / {m_selectedClip.length.ToString("F2")}";

        var box = new Box();
        var slider = new Slider("Time", 0, m_selectedClip.length);
        slider.showInputField = true;
        slider.RegisterValueChangedCallback(v =>
        {
            if (!m_isPlaying)
            {
                StopAnimating();
                m_selectedClip.SampleAnimation(m_selectedAnimator.gameObject, v.newValue);
                m_labelInfo.text = $"{v.newValue.ToString("F2")} / {m_selectedClip.length.ToString("F2")}";
            }
        });

        var speedSlider = new Slider("Speed", 0.1f, 2);
        speedSlider.showInputField = true;
        speedSlider.value = m_currentAnimationSpeed;
        speedSlider.RegisterValueChangedCallback(v =>
        {
            m_currentAnimationSpeed = v.newValue;
        });

        var loopToggle = new Toggle();
        loopToggle.text = "Loop Animation";
        loopToggle.value = m_currentAnimationLoop;
        loopToggle.RegisterValueChangedCallback(v => m_currentAnimationLoop = v.newValue);

        buttonBox.Add(buttonBeginning);
        buttonBox.Add(buttonPlay);
        buttonBox.Add(buttonPause);
        buttonBox.Add(buttonEnding);
        buttonBox.Add(m_labelInfo);

        box.Add(slider);
        box.Add(speedSlider);
        box.Add(loopToggle);

        m_rightPane.Add(buttonBox);
        m_rightPane.Add(box);
    }

    private void OnEditorUpdate()
    {
        if (m_isAnimating)
        {
            if (m_selectedAnimator == null)
                StopAnimating();
            float animTime = (Time.realtimeSinceStartup - m_editorTime) * m_currentAnimationSpeed;
            if (animTime > m_selectedClip.length)
            {
                if (m_currentAnimationLoop)
                {
                    m_editorTime = Time.realtimeSinceStartup;
                }
                else
                {
                    animTime = m_selectedClip.length;
                    StopAnimating();
                }
            }
            if(m_selectedAnimator != null)
                m_selectedClip.SampleAnimation(m_selectedAnimator.gameObject, animTime);
            m_labelInfo.text = $"{animTime.ToString("F2")} / {m_selectedClip.length.ToString("F2")}";
        }
    }

    private void OnPlayModeStateChange(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            StopAnimating();
        }

        if(state == PlayModeStateChange.EnteredPlayMode)
        {
            m_isPlaying = true;
        }

        if(state == PlayModeStateChange.ExitingPlayMode)
        {
            m_isPlaying = false;
        }
    }

    private void StopAnimating()
    {
        AnimationMode.StopAnimationMode();
        EditorApplication.update -= OnEditorUpdate;
        m_isAnimating = false;
    }

    private void RefreshUI()
    {
        //Debug.Log("Hello");
        var allObjects = new List<Animator>();
        allObjects = Object.FindObjectsOfType<Animator>().ToList();

       // m_animatorsListView.itemsSource = null;
        m_animatorsListView.makeItem = () => new Label();
        m_animatorsListView.bindItem = (item, index) => { (item as Label).text = allObjects[index].name; };
        m_animatorsListView.itemsSource = allObjects;

        //m_animatorsListView.selectedIndex = m_selectedAnimatorIndex;
        //m_animatorsListView.onSelectionChange += (items) => { m_selectedAnimatorIndex = m_animatorsListView.selectedIndex; };

        //m_animationsListView.selectedIndex = m_selectedAnimationIndex;
        //m_animationsListView.onSelectionChange += (items) => { m_selectedAnimationIndex = m_animationsListView.selectedIndex; };

        //m_animatorsListView.Refresh();
        m_animationsListView.Refresh();
    }
}
