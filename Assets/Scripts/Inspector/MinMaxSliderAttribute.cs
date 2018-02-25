//Based off of https://gist.github.com/frarees/9791517#file-minmaxsliderattribute-cs-L13

using System;
using UnityEngine;

public class MinMaxSliderAttribute : PropertyAttribute
{

	public readonly float max;
	public readonly float min;

	public MinMaxSliderAttribute(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}