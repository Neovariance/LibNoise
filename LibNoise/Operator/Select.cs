﻿// 
// Copyright (c) 2013 Agustin Santos
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included 
// in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNoise.Operator
{
	/// <summary>
	/// Provides a noise module that outputs the value selected from one of two source
	/// modules chosen by the output value from a control module. [OPERATOR]
	/// </summary>
	public class Select : ModuleBase
	{
		#region Fields

		private double m_fallOff = 0.0;
		private double m_raw = 0.0;
		private double m_min = -1.0;
		private double m_max = 1.0;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of Select.
		/// </summary>
		public Select()
			: base(3)
		{
		}

		/// <summary>
		/// Initializes a new instance of Select.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		/// <param name="fallOff">The falloff value at the edge transition.</param>
		/// <param name="input1">The first input module.</param>
		/// <param name="input2">The second input module.</param>
		/// <param name="controller">The controller of the operator.</param>
		public Select(double min, double max, double fallOff, ModuleBase input1, ModuleBase input2, ModuleBase controller)
			: base(3)
		{
			this.m_modules[0] = input1;
			this.m_modules[1] = input2;
			this.m_modules[2] = controller;
			this.m_min = min;
			this.m_max = max;
			this.FallOff = fallOff;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the controlling module.
		/// </summary>
		public ModuleBase Controller
		{
			get { return this.m_modules[2]; }
			set
			{
				System.Diagnostics.Debug.Assert(value != null);
				this.m_modules[2] = value;
			}
		}

		/// <summary>
		/// Gets or sets the falloff value at the edge transition.
		/// </summary>
		public double FallOff
		{
			get { return this.m_fallOff; }
			set
			{
				double bs = this.m_max - this.m_min;
				this.m_raw = value;
				this.m_fallOff = (value > bs / 2) ? bs / 2 : value;
			}
		}

		/// <summary>
		/// Gets or sets the maximum.
		/// </summary>
		public double Maximum
		{
			get { return this.m_max; }
			set
			{
				this.m_max = value;
				this.FallOff = this.m_raw;
			}
		}

		/// <summary>
		/// Gets or sets the minimum.
		/// </summary>
		public double Minimum
		{
			get { return this.m_min; }
			set
			{
				this.m_min = value;
				this.FallOff = this.m_raw;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sets the bounds.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public void SetBounds(double min, double max)
		{
			System.Diagnostics.Debug.Assert(min < max);
			this.m_min = min;
			this.m_max = max;
			this.FallOff = this.m_fallOff;
		}

		#endregion

		#region ModuleBase Members

		/// <summary>
		/// Returns the output value for the given input coordinates.
		/// </summary>
		/// <param name="x">The input coordinate on the x-axis.</param>
		/// <param name="y">The input coordinate on the y-axis.</param>
		/// <param name="z">The input coordinate on the z-axis.</param>
		/// <returns>The resulting output value.</returns>
		public override double GetValue(double x, double y, double z)
		{
			System.Diagnostics.Debug.Assert(this.m_modules[0] != null);
			System.Diagnostics.Debug.Assert(this.m_modules[1] != null);
			System.Diagnostics.Debug.Assert(this.m_modules[2] != null);
			double cv = this.m_modules[2].GetValue(x, y, z);
			double a;
			if (this.m_fallOff > 0.0)
			{
				if (cv < (this.m_min - this.m_fallOff)) { return this.m_modules[0].GetValue(x, y, z); }
				else if (cv < (this.m_min + this.m_fallOff))
				{
					double lc = (this.m_min - this.m_fallOff);
					double uc = (this.m_min + this.m_fallOff);
					a = Utils.MapCubicSCurve((cv - lc) / (uc - lc));
					return Utils.InterpolateLinear(this.m_modules[0].GetValue(x, y, z), this.m_modules[1].GetValue(x, y, z), a);

				}
				else if (cv < (this.m_max - this.m_fallOff)) { return this.m_modules[1].GetValue(x, y, z); }
				else if (cv < (this.m_max + this.m_fallOff))
				{
					double lc = (this.m_max - this.m_fallOff);
					double uc = (this.m_max + this.m_fallOff);
					a = Utils.MapCubicSCurve((cv - lc) / (uc - lc));
					return Utils.InterpolateLinear(this.m_modules[1].GetValue(x, y, z), this.m_modules[0].GetValue(x, y, z), a);

				}
				return this.m_modules[0].GetValue(x, y, z);
			}
			else
			{
				if (cv < this.m_min || cv > this.m_max) { return this.m_modules[0].GetValue(x, y, z); }
				return this.m_modules[1].GetValue(x, y, z);
			}
		}

		#endregion
	}
}