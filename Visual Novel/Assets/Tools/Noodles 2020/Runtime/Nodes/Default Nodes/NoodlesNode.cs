﻿using System;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace nullbloq.Noodles
{
	/// <summary>
	/// Noodles Node Model Base class
	/// </summary>
	[Serializable]
	public class NoodlesNode
	{
		public string title;
		public string GUID;
		public Rect rect = new Rect(0,0,100,100);
		public int width = 300;
		public int height = 200;

		public List<NoodlesPort> inputPorts = new List<NoodlesPort>();
		public List<NoodlesPort> outputPorts = new List<NoodlesPort>();

		[HideInInspector]
		public string classNameString;

		public NoodlesNode()
		{
			Debug.Log("NoodlesNode Constructor");
#if UNITY_EDITOR
			classNameString = typeof(NoodlesNodeVisual).AssemblyQualifiedName;
#endif
		}
		public void Set(Vector2 pos, string _GUID)
		{
			GUID = _GUID;
			rect = new Rect(pos.x, pos.y, width, height);
			PostSet();
		}
		protected virtual void PostSet() { }

		public void RemovePort(NoodlesPort p)
		{
			if (inputPorts.Contains(p))
				inputPorts.Remove(p);
			if (outputPorts.Contains(p))
				outputPorts.Remove(p);
		}

		public NoodlesPort GetPort(string _portGUID)
		{
			for (var i = 0; i < outputPorts.Count; i++)
			{
				NoodlesPort noodlesPort = outputPorts[i];
				if (noodlesPort.GUID == _portGUID)
					return noodlesPort;
			}

			return null;
		}

		public string GetNextNodeID(int index)
		{
			if (index < outputPorts.Count)
				return outputPorts[index].targetNodeGUID[0]; //TODO mmmmmm?

			Debug.LogError("Asking for data in unavailable Port");

			return null;
		}

		public bool HasAnyOutput()
		{
			return outputPorts != null && outputPorts.Count > 0;
		}

	}
#if UNITY_EDITOR
	/// <summary>
	/// Noodles Node View Base class, extending from Node
	/// Initialices based on Noodle Node data.
	/// </summary>
	public class NoodlesNodeVisual : Node
	{
		public IEdgeConnectorListener listener;
		public NoodlesNode nodeData;
		public List<NoodlesPortVisual> inputPorts = new List<NoodlesPortVisual>();
		public List<NoodlesPortVisual> outputPorts = new List<NoodlesPortVisual>();

		public void InitializeWithNoodlesNode(NoodlesNode _nodeData)
		{
			nodeData = _nodeData;
			CreateVisualsHeader();
			CreateVisualsBody();
			CreateVisualsFooter();
		}

		protected virtual void CreateVisualsHeader()
		{
			styleSheets.Add(Resources.Load<StyleSheet>("Node"));
		}

		protected virtual void CreateVisualsBody()
		{
			CreateVisualPorts();
		}

		protected virtual void CreateVisualsFooter()
		{
			SetPosition(nodeData.rect);
			RefreshExpandedState();
			RefreshPorts();
			SetPosition(nodeData.rect);
		}

		protected void CreateVisualPorts()
		{
			foreach (NoodlesPort inPort in nodeData.inputPorts)
			{
				NoodlesPortVisual inputPort = CreateVisualPort(inPort, Direction.Input);
				inputContainer.Add(inputPort);
			}

			foreach (NoodlesPort outPort in nodeData.outputPorts)
			{
				NoodlesPortVisual outputPort = CreateVisualPort(outPort, Direction.Output);
				outputContainer.Add(outputPort);
			}
		}

		protected NoodlesPortVisual CreateVisualPort(NoodlesPort portData, Direction dir)
		{
			NoodlesPortVisual newPort = NoodlesPortVisual.CreateVisualPort(portData, Orientation.Horizontal, dir, Port.Capacity.Multi, typeof(float), listener);
			newPort.InitializeWithNoodlesNode(portData, OnRemoveVisualPort);
			if (dir == Direction.Input)
				inputPorts.Add(newPort);
			else
				outputPorts.Add(newPort);
			return newPort;
		}

		protected virtual void OnRemoveVisualPort(NoodlesPort p, NoodlesPortVisual pv)
		{
			nodeData.RemovePort(p);
			if (inputPorts.Contains(pv))
				inputPorts.Remove(pv);
			if (outputPorts.Contains(pv))
				outputPorts.Remove(pv);
			Debug.Log("TODO remove Edges");
		}

		public NoodlesPortVisual GetPortWithGUID(string portGUID)
		{
			foreach (NoodlesPortVisual inputPort in inputPorts)
			{
				if (inputPort.portData.GUID == portGUID)
					return inputPort;
			}
			foreach (NoodlesPortVisual outputPort in outputPorts)
			{
				if (outputPort.portData.GUID == portGUID)
					return outputPort;
			}

			return null;
		}

		public void Save_MOMENTARY_()
		{
			nodeData.rect = GetPosition();
		}
	}
#endif
}