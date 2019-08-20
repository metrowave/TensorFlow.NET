﻿/*****************************************************************************
   Copyright 2018 The TensorFlow.NET Authors. All Rights Reserved.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************/

using System;
using System.Collections.Generic;
using static Tensorflow.Python;
using static Tensorflow.Binding;

namespace Tensorflow.Keras
{
    public class backend : BackendBase
    {
        /* ----------------------------------------  KERAS BACKEND NATIVE OBJECTS  ---------------------------------------- */
        public static Func<Array, double> py_sum = sum;
        public static Func<Array, bool> py_all = all;
        //Func<Array, bool> py_any = any;
        //Func<double, double, double, IEnumerable<double>> py_slice = slice;

        public static Session _SESSION = tf.defaultSession;
        public static Graph _GRAPH = null;
        public static Dictionary<Graph, GraphLearningPhase> _GRAPH_LEARNING_PHASES;
        //Dictionary<Graph, Dictionary<string, int>> PER_GRAPH_LAYER_NAME_UIDS;
        public static bool _MANUAL_VAR_INIT = false;
        public static List<string> _LOCAL_DEVICES = null;
        /* --------------------------------------  KERAS BACKEND NATIVE OBJECTS END  -------------------------------------- */

        /// <summary>
        /// A global dictionary mapping graph objects to an index of counters used
        /// for various layer names in each graph.
        /// Allows to give unique autogenerated names to layers, in a graph-specific way.
        /// </summary>
        public static Dictionary<Graph, Dictionary<(string, string), int>> PER_GRAPH_LAYER_NAME_UIDS = new Dictionary<Graph, Dictionary<(string, string), int>>();
        public static Dictionary<string, RefVariable> _GRAPH_VARIABLES = new Dictionary<string, RefVariable>();
        public static Dictionary<string, Optimizer> _GRAPH_TF_OPTIMIZERS = new Dictionary<string, Optimizer>();

        public static _DummyEagerGraph _DUMMY_EAGER_GRAPH = new _DummyEagerGraph();

        public static void track_variable(RefVariable v)
        {
            var graph = v.graph;
            _GRAPH_VARIABLES[graph.graph_key] = v;
        }

        public static Tensor placeholder(int[] shape = null,
            int ndim = -1,
            TF_DataType dtype = TF_DataType.DtInvalid,
            bool sparse = false,
            string name = null)
        {
            if (sparse)
            {
                throw new NotImplementedException("placeholder sparse is true");
            }
            else
            {
                return gen_array_ops.placeholder(dtype: dtype, shape: new TensorShape(shape), name: name);
            }
        }

        public static Graph get_graph()
        {
            return ops.get_default_graph();
        }

        public static int get_uid(string prefix, string @namespace = "")
        {
            var graph = tf.get_default_graph();
            if (!PER_GRAPH_LAYER_NAME_UIDS.ContainsKey(graph))
                PER_GRAPH_LAYER_NAME_UIDS.Add(graph, new defaultdict<(string, string), int>());
            PER_GRAPH_LAYER_NAME_UIDS[graph][(@namespace, prefix)] += 1;

            return PER_GRAPH_LAYER_NAME_UIDS[graph][(@namespace, prefix)];
        }
        public static int get_uid((string, string) name)
        {
            var graph = tf.get_default_graph();
            if (!PER_GRAPH_LAYER_NAME_UIDS.ContainsKey(graph))
                PER_GRAPH_LAYER_NAME_UIDS.Add(graph, new defaultdict<(string, string), int>());
            PER_GRAPH_LAYER_NAME_UIDS[graph][(name)] += 1;

            return PER_GRAPH_LAYER_NAME_UIDS[graph][name];
        }
        public static void reset_uids() => PER_GRAPH_LAYER_NAME_UIDS = new Dictionary<Graph, Dictionary<(string, string), int>>();
        public static void clear_session()
        {
            ops.reset_default_graph();
            reset_uids();
            _SESSION = null;
            var phase = tf.placeholder_with_default(false, new int[] { }, name: "keras_learning_phase");
            _GRAPH_LEARNING_PHASES = new Dictionary<Graph, GraphLearningPhase>();
            _GRAPH_LEARNING_PHASES[tf.get_default_graph()] = 0;
        }
        public static void manual_variable_initialization(bool value)
        {
            _MANUAL_VAR_INIT = value;
        }
        public static GraphLearningPhase learning_phase()
        {
            var graph = tf.get_default_graph();
            if (_GRAPH_LEARNING_PHASES.ContainsKey(graph))
            {
                var phase = tf.placeholder_with_default(false, shape: new int[] { }, name: "keras_learning_phase");
                _GRAPH_LEARNING_PHASES[graph] = 0;
            }
            return _GRAPH_LEARNING_PHASES[graph];
        }
        public static void set_learning_phase(bool value)
        {
            _GRAPH_LEARNING_PHASES[tf.get_default_graph()] = (GraphLearningPhase)((value) ? 1 : 0);
        }


        public class _DummyEagerGraph
        { }
    }
}
