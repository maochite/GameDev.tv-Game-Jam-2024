using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Chunk))]
public class ChunkEditorInput : Editor
{
    void OnSceneGUI()
    {
        if (MapEditor.Instance == null)
        {
            return;
        }

        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        switch (Event.current.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (Event.current.button == 0)
                {
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                    MapEditor.Instance.HandleClick(ray, Event.current.shift);
                }
                break;
            case EventType.MouseUp:
                if (Event.current.button == 0)
                {
                    //GUIUtility.hotControl = controlID;
                    //Event.current.Use();
                }
                break;
            case EventType.KeyDown:
                if (MapEditor.Instance.HandleKeyDown(Event.current.keyCode))
                {
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                }
                break;
            case EventType.KeyUp:
                if (MapEditor.Instance.HandleKeyUp(Event.current.keyCode))
                {
                    //GUIUtility.hotControl = controlID;
                    //Event.current.Use();
                }
                break;
        }

        MapEditor.Instance.UpdateSelection(ray);
        MapEditor.Instance.DrawOverlay();
        SceneView.RepaintAll();
    }
}

public class PlayerInput : MonoBehaviour
{
    private static PlayerInput m_instance;
    public static PlayerInput Instance { get => m_instance; }
    private bool m_shiftHeld;
    public bool ShiftHeld { get => m_shiftHeld; }

    void Start()
    {
        m_instance = this;
        m_shiftHeld = false;
    }

    // Update is called once per frame
    void Update()
    {
        m_shiftHeld = Input.GetKey(KeyCode.LeftShift);
    }
}
