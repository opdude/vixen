﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CommonElements.Timeline;
using Vixen.Module.Editor;
using Vixen.Sys;

namespace VixenModules.Editor.TimedSequenceEditor
{
	class TimedSequenceElement : TimelineElement
	{
		public CommandNode CommandNode { get; set; }
	}
}