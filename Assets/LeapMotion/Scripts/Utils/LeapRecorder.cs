﻿using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using Leap;

public enum RecorderState {
  Idling = 0,
  Recording = 1,
  Playing = 2
}

public class LeapRecorder {
  public int startTime = 0;
  public float speed = 1.0f;
  public bool loop = true;
  public int delay = 0;
  public RecorderState state = RecorderState.Idling;

  private List<byte[]> frames_;
  private float frame_index_;
  
  public LeapRecorder() {
    Reset();
  }
  
  public void Reset() {
    frames_ = new List<byte[]>();
    frame_index_ = 0;
    state = RecorderState.Idling;
  }
  
  public void SetDefault() {
    startTime = 0;
    speed = 1.0f;
    loop = true;
    delay = 0;
  }

  public int GetIndex() { return (int)frame_index_; }
  public void SetIndex(int new_index) { 
    if (new_index >= frames_.Count) {
      frame_index_ = frames_.Count - 1;
    }
    else {
      frame_index_ = new_index; 
    }
  }
  
  public void AddFrame(Frame frame) {
    frames_.Add(frame.Serialize);
  }
  
  public Frame GetFrame() {
    Frame frame = new Frame();
    if (frame_index_ < 0) {
      frame_index_++;
    }
    else if (frames_.Count > 0) {
      if (frame_index_ >= frames_.Count) {
        if (loop) {
          frame_index_ = -delay;
        }
        else {
          return frame;
        }
      }
      if (frame_index_ >= 0) {
        frame.Deserialize(frames_[(int)frame_index_]);
        frame_index_ += speed;
      }
    }
    return frame;
  }
  
  public List<Frame> GetFrames() {
    List<Frame> frames = new List<Frame>();
    for (int i = 0; i < frames_.Count; ++i) {
      Frame frame = new Frame();
      frame.Deserialize(frames_[i]);
      frames.Add(frame);
    }
    return frames;
  }
  
  public int GetFramesCount() {
    return frames_.Count;
  }
  
  public TextAsset Save(string path) {
    if (File.Exists(@path)) {
      File.Delete(@path);
    }

    FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write);
    for (int i = 0; i < frames_.Count; ++i) {
      byte[] frame_size = new byte[4];
      frame_size = System.BitConverter.GetBytes(frames_[i].Length);
      stream.Write(frame_size, 0, frame_size.Length);
      stream.Write(frames_[i], 0, frames_[i].Length);
    }
    
    stream.Close();
    AssetDatabase.Refresh();
    return (TextAsset)Resources.LoadAssetAtPath(path, typeof(TextAsset));
  }
  
  public void Load(TextAsset text_asset) {
    byte[] data = text_asset.bytes;
    frame_index_ = -startTime;
    frames_.Clear();
    int i = 0;
    while (i < data.Length) {
      byte[] frame_size = new byte[4];
      Array.Copy(data, i, frame_size, 0, frame_size.Length);
      i += frame_size.Length;
      byte[] frame = new byte[System.BitConverter.ToUInt32(frame_size, 0)];
      Array.Copy(data, i, frame, 0, frame.Length);
      i += frame.Length;
      frames_.Add(frame);
    }
  }
}