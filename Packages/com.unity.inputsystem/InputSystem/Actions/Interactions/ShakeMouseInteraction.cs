using System;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Scripting;
using System.Drawing.Text;
using UnityEditor.Experimental.GraphView;


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem.Editor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace UnityEngine.InputSystem.Interactions
{
    /// <summary>
    /// Interaction that requires multiple taps (press and release within <see cref="tapTime"/>) spaced no more
    /// than <see cref="tapDelay"/> seconds apart. This equates to a chain of <see cref="TapInteraction"/> with
    /// a maximum delay between each tap.
    /// </summary>
    /// <remarks>
    /// The interaction goes into <see cref="InputActionPhase.Started"/> on the first press and then will not
    /// trigger again until either the full tap sequence is performed (in which case the interaction triggers
    /// <see cref="InputActionPhase.Performed"/>) or the multi-tap is aborted by a timeout being hit (in which
    /// case the interaction will trigger <see cref="InputActionPhase.Canceled"/>).
    /// </remarks>
    public class ShakeMouseInteraction : IInputInteraction<Vector2>
    {
        public const float DefaultSwerveTime = 0.2f;
        public const float DefaultSwerveDelay = 0.8f;

        /// <summary>
        /// The time in seconds within which the control needs to be pressed and released to perform the interaction.
        /// </summary>
        /// <remarks>
        /// If this value is equal to or smaller than zero, the input system will use (<see cref="DefaultSwerveTime"/>) instead.
        /// </remarks>
        [Tooltip("The maximum time (in seconds) allowed to elapse between a change of direction of mouse movement for it to register as a swerve.")]
        public float swerveTime;

        /// <summary>
        /// The time in seconds within which the control needs to be pressed and released to perform the interaction.
        /// </summary>
        /// <remarks>
        /// If this value is equal to or smaller than zero, the input system will use (<see cref="DefaultSwerveDelay"/>) instead.
        /// </remarks>
        [Tooltip("The maximum time (in seconds) allowed to elapse between swerves.")]
        public float swerveDelay;

        /// <summary>
        /// The number of taps required to perform the interaction.
        /// </summary>
        /// <remarks>
        /// How many taps need to be performed in succession. Two means double-tap, three means triple-tap, and so on.
        /// </remarks>
        [Tooltip("How many sudden changes of direction (swerves) need to be performed in succession.")]
        public int swerveCount = 2;

        /// <summary>
        /// Direction velocity threshold that must be crossed for a direction angle to be valid
        /// </summary>
        /// <remarks>
        /// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultButtonPressPoint"/> is used instead.
        /// </remarks>
        [Tooltip("Direction velocity threshold that must be crossed for a direction angle to be valid.")]
        public float moveMagnitudeThreshold;

        /// <summary>
        /// Number of direction regions for registering a change of direction
        /// </summary>
        /// <remarks>
        /// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultButtonPressPoint"/> is used instead.
        /// </remarks>
        [Tooltip("Direction velocity threshold that must be crossed for a direction angle to be valid.")]
        public int numberOfDirectionRegions;

        private float swerveTimeOrDefault => swerveTime > 0.0 ? swerveTime : DefaultSwerveTime;
        private float swerveDelayOrDefault => swerveDelay > 0.0 ? swerveDelay : DefaultSwerveDelay;


        // ToDo: Remove
        private float pressPointOrDefault => 1.0f; // pressPoint > 0 ? pressPoint : ButtonControl.s_GlobalDefaultButtonPressPoint;
        private float releasePointOrDefault => 1.0f; // pressPointOrDefault * ButtonControl.s_GlobalDefaultButtonReleaseThreshold;

        /// <inheritdoc />
        public void Process(ref InputInteractionContext context)
        {
            return;
            if (context.timerHasExpired)
            {
                // We use timers multiple times but no matter what, if they expire it means
                // that we didn't get input in time.
                context.Canceled();
                Debug.Log("####### Canceled 1");
                return;
            }

            switch (m_CurrentTapPhase)
            {
                case ShakePhase.WaitingForFirstSwerve:
                    IsMoveLarge(context.ReadValue<Vector2>(), out var direction);

                    // if (context.ControlIsActuated(pressPointOrDefault))
                    {
                        m_CurrentTapPhase = ShakePhase.WaitingForNextRelease;
                        m_CurrentTapStartTime = context.time;
                        context.Started();
                        var value = context.ReadValue<Vector2>();
                        // Debug.Log("####### Started");

                        var maxTapTime = swerveTimeOrDefault;
                        var maxDelayInBetween = swerveDelayOrDefault;
                        context.SetTimeout(maxTapTime);

                        // We'll be using multiple timeouts so set a total completion time that
                        // effects the result of InputAction.GetTimeoutCompletionPercentage()
                        // such that it accounts for the total time we allocate for the interaction
                        // rather than only the time of one single timeout.
                        context.SetTotalTimeoutCompletionTime(maxTapTime * swerveCount + (swerveCount - 1) * maxDelayInBetween);
                    }
                    break;

                case ShakePhase.WaitingForNextRelease:
                    // if (!context.ControlIsActuated(releasePointOrDefault))
                    {
                        if (context.time - m_CurrentTapStartTime <= swerveTimeOrDefault)
                        {
                            ++m_CurrentswerveCount;
                            if (m_CurrentswerveCount >= swerveCount)
                            {
                                context.Performed();
                                // Debug.Log("####### Performed");
                            }
                            else
                            {
                                m_CurrentTapPhase = ShakePhase.WaitingForNextRelease;
                                m_LastTapReleaseTime = context.time;
                                context.SetTimeout(swerveDelayOrDefault);
                            }
                        }
                        else
                        {
                            context.Canceled();
                            Debug.Log("####### Canceled 2");
                        }
                    }
                    break;

                //case TapPhase.WaitingForNextPress:
                //    if (context.ControlIsActuated(pressPointOrDefault))
                //    {
                //        if (context.time - m_LastTapReleaseTime <= swerveDelayOrDefault)
                //        {
                //            m_CurrentTapPhase = TapPhase.WaitingForNextRelease;
                //            m_CurrentTapStartTime = context.time;
                //            context.SetTimeout(swerveTimeOrDefault);
                //        }
                //        else
                //        {
                //            context.Canceled();
                //        }
                //    }
                //    break;
            }
        }

        private bool IsMoveLarge(Vector2 delta, out MoveDirection direction)
        {
            if (delta.magnitude > moveMagnitudeThreshold)
            {
                direction = MoveDirection.Right;
                return true;
            }
            direction = MoveDirection.None;
            return false;
        }

        /// <inheritdoc />
        public void Reset()
        {
            return;
            m_CurrentTapPhase = ShakePhase.WaitingForFirstSwerve;
            m_CurrentswerveCount = 0;
            m_CurrentTapStartTime = 0;
            m_LastTapReleaseTime = 0;
        }

        private ShakePhase m_CurrentTapPhase;
        private int m_CurrentswerveCount;
        private double m_CurrentTapStartTime;
        private double m_LastTapReleaseTime;

        private enum ShakePhase
        {
            WaitingForFirstSwerve,
            WaitingForNextRelease,
            // WaitingForNextPress,
        }

        private enum MoveDirection
        {
            None,
            Left,
            Right,
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// UI that is displayed when editing <see cref="ShakeMouseInteraction"/> in the editor.
    /// </summary>
    internal class ShakeMouseInteractionEditor : InputParameterEditor<ShakeMouseInteraction>
    {
        protected override void OnEnable()
        {
            m_TapTimeSetting.Initialize("Max Tap Duration",
                "Time (in seconds) within with a control has to be released again for it to register as a tap. If the control is held "
                + "for longer than this time, the tap is canceled.",
                "Default Tap Time",
                () => target.swerveTime, x => target.swerveTime = x, () => ShakeMouseInteraction.DefaultSwerveTime);
            m_TapDelaySetting.Initialize("Max Tap Spacing",
                "The maximum delay (in seconds) allowed between each tap. If this time is exceeded, the multi-tap is canceled.",
                "Default Tap Spacing",
                () => target.swerveDelay, x => target.swerveDelay = x, () => InputSystem.settings.multiTapDelayTime);
            m_PressPointSetting.Initialize("Press Point",
                "The amount of actuation a control requires before being considered pressed. If not set, default to "
                + "'Default Button Press Point' in the global input settings.",
                "Default Button Press Point",
                () => target.moveMagnitudeThreshold, v => target.moveMagnitudeThreshold = v,
                () => InputSystem.settings.defaultButtonPressPoint);
        }

        public override void OnGUI()
        {
#if UNITY_INPUT_SYSTEM_PROJECT_WIDE_ACTIONS
            if (!InputSystem.settings.IsFeatureEnabled(InputFeatureNames.kUseIMGUIEditorForAssets)) return;
#endif
            target.swerveCount = EditorGUILayout.IntField(m_swerveCountLabel, target.swerveCount);
            m_TapDelaySetting.OnGUI();
            m_TapTimeSetting.OnGUI();
            m_PressPointSetting.OnGUI();
        }

#if UNITY_INPUT_SYSTEM_PROJECT_WIDE_ACTIONS
        public override void OnDrawVisualElements(VisualElement root, Action onChangedCallback)
        {
            var swerveCountField = new IntegerField(m_swerveCountLabel.text)
            {
                value = target.swerveCount,
                tooltip = m_swerveCountLabel.tooltip
            };
            swerveCountField.RegisterValueChangedCallback(evt =>
            {
                target.swerveCount = evt.newValue;
                onChangedCallback?.Invoke();
            });
            root.Add(swerveCountField);

            m_TapDelaySetting.OnDrawVisualElements(root, onChangedCallback);
            m_TapTimeSetting.OnDrawVisualElements(root, onChangedCallback);
            m_PressPointSetting.OnDrawVisualElements(root, onChangedCallback);
        }

#endif

        private readonly GUIContent m_swerveCountLabel = new GUIContent("Tap Count", "How many taps need to be performed in succession. Two means double-tap, three means triple-tap, and so on.");

        private CustomOrDefaultSetting m_PressPointSetting;
        private CustomOrDefaultSetting m_TapTimeSetting;
        private CustomOrDefaultSetting m_TapDelaySetting;
    }
#endif
}
