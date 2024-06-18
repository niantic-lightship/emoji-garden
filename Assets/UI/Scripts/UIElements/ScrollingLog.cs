// Copyright 2022-2024 Niantic.
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Niantic.Lightship.AR.Samples
{
  // Simple scrolling window that prints to the application screen whatever is printed through
  // calls to the UnityEngine.Debug.Log method.
  [DefaultExecutionOrder(Int32.MinValue)]
  public class ScrollingLog:
    MonoBehaviour
  {
    /// Font size for log text entries. Spacing between log entries is also set to half this value.
    [SerializeField]
    private GameObject _logRoot;

    /// Layout box containing the log entries
    [SerializeField]
    private VerticalLayoutGroup LogHistory = null;

    /// Log entry prefab used to generate new entries when requested
    [SerializeField]
    private Text LogEntryPrefab = null;

    [SerializeField]
    private ScrollRect _scrollRect;

    private static List<Text> _logEntryObjs = new();

    private static readonly List<string> _logEntries = new();
    private static ScrollingLog _instance = null;
    private const int MaxLogCount = 100;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
      Application.logMessageReceived += StaticAddLogEntry;
    }

    private static void StaticAddLogEntry(string str, string stackTrace, LogType type)
    {
      if (_logEntries.Count >= MaxLogCount)
      {
        _logEntries.RemoveAt(0);
      }

      _logEntries.Add(str);

      if (_instance != null)
      {
        _instance.AddLogEntry(str);
      }
    }

    private static void SetupNewLog(ScrollingLog newLog)
    {
      _instance = newLog;
      foreach (var entry in _logEntries)
      {
        newLog.AddLogEntry(entry);
      }
    }

    private static void TearDownLog(ScrollingLog newLog)
    {
      _instance = null;
      newLog.Clear();
    }

    // Creates a new log entry using the provided string.
    private void AddLogEntry(string str)
    {
      var newLogEntry = Instantiate(LogEntryPrefab, LogHistory.transform);
      newLogEntry.text = str;
      _logEntryObjs.Add(newLogEntry);
      ScrollToBottom();
    }

    private void Clear()
    {
      foreach (var entry in _logEntryObjs)
        Destroy(entry.gameObject);

      _logEntryObjs.Clear();
    }

    private void ScrollToBottom()
    {
      _scrollRect.verticalNormalizedPosition = 0.0f;
    }

    public void ShowLog()
    {
      SetupNewLog(this);
      _logRoot.SetActive(true);
      ScrollToBottom();
    }

    public void HideLog()
    {
      _logRoot.SetActive(false);
      TearDownLog(this);
    }

    protected void LateUpdate()
    {
      transform.SetAsLastSibling();
    }
  }
}
