using System;


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Editor;
using UnityEngine.UIElements;
#endif

namespace UnityEngine.InputSystem.Interactions
{
    /// <summary>
    /// Interaction that requires multiple swerves (swift move from right to left, or left to right, with the mouse).
    /// One swerve need to be done within <see cref="swerveTime"/>) spaced no more than <see cref="swerveDelay"/>
    /// seconds apart. This equates to a chain of <see cref="ShakeMouseInteraction"/> with a maximum delay between each swerve.
    /// </summary>
    /// <remarks>
    /// The interaction goes into <see cref="InputActionPhase.Started"/> when two swift moves in opposite directions are detected.
    /// It will remain processing mouse updates continously in <see cref="InputActionPhase.Waiting"/>.
    /// After detecting <see cref="swerveCount"/> swerves in a row it will trigger <see cref="InputActionPhase.Performed"/>.
    /// If any wait times out the interaction is aborted and <see cref="InputActionPhase.Canceled"/> will be triggered.
    /// </remarks>
    public class ShakeMouseInteraction : IInputInteraction<Vector2>
    {
        public const float DefaultSwerveTime = 0.2f;
        public const float DefaultSwerveDelay = 0.8f;
        public const float DefaultMoveMagnitudeThreshold = 0.1f;
        public const int DefaultSwerveCount = 8;

        /// <summary>
        /// The time in seconds within which a swift left and right move needs to be done to detect a swerve.
        /// </summary>
        /// <remarks>
        /// If this value is equal to or smaller than zero, the input system will use (<see cref="DefaultSwerveTime"/>) instead.
        /// </remarks>
        [Tooltip("The maximum time (in seconds) allowed to elapse between a change of direction of mouse movement for it to register as a swerve.")]
        public float swerveTime;

        /// <summary>
        /// The time in seconds within which any new swerve needs to be detected to continue the interaction.
        /// </summary>
        /// <remarks>
        /// If this value is equal to or smaller than zero, the input system will use (<see cref="DefaultSwerveDelay"/>) instead.
        /// </remarks>
        [Tooltip("The maximum time (in seconds) allowed to elapse between swerves.")]
        public float swerveDelay;

        /// <summary>
        /// The number of sweves required to perform the interaction.
        /// </summary>
        /// <remarks>
        /// How many swerves need to be done in succession (<see cref="DefaultSwerveCount"/>).
        /// </remarks>
        [Tooltip("How many swift changes of direction (swerves) need to be performed in succession.")]
        public int swerveCount;

        /// <summary>
        /// Direction velocity threshold that must be crossed for a move to be considered
        /// </summary>
        /// <remarks>
        /// If this is less than or equal to 0 (the default), <see cref="DefaultMoveMagnitudeThreshold"/> is used instead.
        /// </remarks>
        [Tooltip("Direction velocity threshold that must be crossed for a direction angle to be valid.")]
        public float moveMagnitudeThreshold;


        private float swerveTimeOrDefault => swerveTime > 0.0 ? swerveTime : DefaultSwerveTime;
        private float swerveDelayOrDefault => swerveDelay > 0.0 ? swerveDelay : DefaultSwerveDelay;
        private float swerveCountOrDefault => swerveCount > 0.0 ? swerveCount : DefaultSwerveCount;
        private float moveMagnitudeThresholdOrDefault => moveMagnitudeThreshold > 0.0 ? moveMagnitudeThreshold : DefaultMoveMagnitudeThreshold;


        /// <inheritdoc />
        public void Process(ref InputInteractionContext context)
        {
            MoveDirection direction;

            if (context.timerHasExpired)
            {
                // We use timers multiple times but no matter what, if they expire it means
                // that we didn't get input in time.
                context.Canceled();
                return;
            }

            switch (m_CurrentShakePhase)
            {
                case ShakePhase.WaitingForFirstSwerve:
                    if (IsMoveSwift(context.ReadValue<Vector2>(), out direction) &&
                        direction != MoveDirection.None)
                    {
                        var now = context.time;

                        if (direction != m_LastDirection &&
                            m_LastDirection != MoveDirection.None &&
                            now < m_LastTimeOfMove + swerveTimeOrDefault)
                        {
                            m_CurrentSwerveCount = 1;
                            m_CurrentShakePhase = ShakePhase.WaitingForAnotherSwerve;
                            context.Started();
                            context.SetTimeout(swerveDelayOrDefault);
                        }
                        m_LastDirection = direction;
                        m_LastTimeOfMove = now;
                    }

                    // We'll be using multiple timeouts so set a total completion time that
                    // effects the result of InputAction.GetTimeoutCompletionPercentage()
                    // such that it accounts for the total time we allocate for the interaction
                    // rather than only the time of one single timeout.

                    // context.SetTotalTimeoutCompletionTime(swerveTime * swerveCountOrDefault + (swerveCountOrDefault - 1) * swerveDelay);
                    break;

                case ShakePhase.WaitingForAnotherSwerve:
                    if (IsMoveSwift(context.ReadValue<Vector2>(), out direction) &&
                        direction != MoveDirection.None)
                    {
                        var now = context.time;

                        if (direction != m_LastDirection &&
                            m_LastDirection != MoveDirection.None &&
                            now < m_LastTimeOfMove + swerveTimeOrDefault)
                        {
                            if (++m_CurrentSwerveCount >= swerveCountOrDefault)
                                context.Performed();
                            else
                                context.SetTimeout(swerveDelayOrDefault);
                        }
                        m_LastDirection = direction;
                        m_LastTimeOfMove = now;
                    }
                    break;
            }
        }

        /// <inheritdoc />
        public void Reset()
        {
            m_CurrentShakePhase = ShakePhase.WaitingForFirstSwerve;
            m_CurrentSwerveCount = 0;
            m_LastDirection = MoveDirection.None;
            m_LastTimeOfMove = 0;
        }

        private bool IsMoveSwift(Vector2 delta, out MoveDirection direction)
        {
            direction = MoveDirection.None;
            if (delta.magnitude > moveMagnitudeThresholdOrDefault)
            {
                const float sqrt3div3 = 0.577f;     // sqrt(3) / 3, tan(30)

                if (delta.x > 0 && Math.Abs(delta.y) < delta.x * sqrt3div3)     // < 30 degrees to y-axis
                    direction = MoveDirection.Right;
                else if (delta.x < 0 && Math.Abs(delta.y) < (-delta.x) * sqrt3div3)
                    direction = MoveDirection.Left;

                return true;
            }
            return false;
        }

        private ShakePhase m_CurrentShakePhase;
        private MoveDirection m_LastDirection = MoveDirection.None;
        private double m_LastTimeOfMove;
        private int m_CurrentSwerveCount;

        private enum ShakePhase
        {
            WaitingForFirstSwerve,
            WaitingForAnotherSwerve,
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
        public override void OnGUI()
        {
#if UNITY_INPUT_SYSTEM_PROJECT_WIDE_ACTIONS
            if (!InputSystem.settings.IsFeatureEnabled(InputFeatureNames.kUseIMGUIEditorForAssets)) return;
#endif
            target.swerveTime = EditorGUILayout.FloatField(m_swerveTimeLabel, target.swerveTime);
            target.swerveDelay = EditorGUILayout.FloatField(m_swerveDelayLabel, target.swerveDelay);
            target.swerveCount = EditorGUILayout.IntField(m_swerveCountLabel, target.swerveCount);
        }

#if UNITY_INPUT_SYSTEM_PROJECT_WIDE_ACTIONS
        public override void OnDrawVisualElements(VisualElement root, Action onChangedCallback)
        {
            var swerveTimeField = new FloatField(m_swerveTimeLabel.text)
            {
                value = target.swerveTime,
                tooltip = m_swerveTimeLabel.tooltip
            };
            swerveTimeField.RegisterValueChangedCallback(evt =>
            {
                target.swerveTime = evt.newValue;
                onChangedCallback?.Invoke();
            });
            root.Add(swerveTimeField);

            var swerveDelayField = new FloatField(m_swerveDelayLabel.text)
            {
                value = target.swerveDelay,
                tooltip = m_swerveDelayLabel.tooltip
            };
            swerveDelayField.RegisterValueChangedCallback(evt =>
            {
                target.swerveDelay = evt.newValue;
                onChangedCallback?.Invoke();
            });
            root.Add(swerveDelayField);

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
        }

#endif

        private readonly GUIContent m_swerveTimeLabel = new GUIContent("Swerve Time", "The maximum time (in seconds) allowed to elapse between a change of direction of mouse movement for it to register as a swerve.");
        private readonly GUIContent m_swerveDelayLabel = new GUIContent("Swerve Delay", "The maximum delay (in seconds) allowed between each swerve. If this time is exceeded, the shake is canceled.");
        private readonly GUIContent m_swerveCountLabel = new GUIContent("Swerve Count", "How many swerves need to be performed in succession.");
    }
#endif
}
