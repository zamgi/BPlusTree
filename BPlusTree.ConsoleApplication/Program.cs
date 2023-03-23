using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Running;

namespace BPlusTree.ConsoleApplication
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class Int32Comparer : IComparer< int >
    {
        public static Int32Comparer Inst { get; } = new Int32Comparer();
        public int Compare( int x, int y ) => (x - y);
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class _StringComparer : IComparer< string >
    {
        public static _StringComparer Inst { get; } = new _StringComparer();
        public int Compare( string x, string y ) => string.Compare( x, y );// string.CompareOrdinal( x, y );
    }
    /// <summary>
    /// 
    /// </summary>
    internal sealed class StartsWithStringComparer : IBPlusTreeComparer< string >
    {
        public static StartsWithStringComparer Inst { get; } = new StartsWithStringComparer();
        public int Compare( string existsInTreeValue, string searchingValue )
        {
            if ( searchingValue.Length <= existsInTreeValue.Length )
            {
                return (string.Compare( existsInTreeValue, 0, searchingValue, 0, searchingValue.Length, true ));
            }
            return (string.Compare( existsInTreeValue, searchingValue, true ));

            //return (string.Compare( value, 0, key, 0, Math.Min( value.Length, key.Length ), true));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
        private static void Test_1()
        {
            const int COUNT = 1_000_000;
            
            //-1-
            var rnd = new Random();
            var bptree = new BPlusTreeList< int >( Int32Comparer.Inst, (int) (COUNT / 1000.0 + 0.5) + 25, 1000 );
            var hs     = new HashSet< int >();
            for ( var i = 1; i <= COUNT; i++ )
            {
                var t = rnd.Next( 0, COUNT ); //i; //
                var r = bptree.TryAdd( t );
                hs.Add( t );
            }

            //-2-
            var lst = new List< int >( hs );
            lst.Sort();
            var array = bptree.ToArray();
            #region compare
            /*if ( !lst.SequenceEqual( array ) )
            {
                for ( int i = 0, len = Math.Min( lst.Count, array.Length ); i < len; i++ )
                {
                    if ( lst[ i ] != array[ i ] )
                    {
                        Debugger.Break();
                    }
                }
            }*/
            #endregion
            Debug.Assert( lst.SequenceEqual( array ), "!lst.SequenceEqual( bptree )" );

            //-3-
            int min = 10, max = 77;
            var arrayBetween1 = bptree.GetValuesBetween( min, max ).ToArray();
            var arrayBetween2 = array.Where( i => min <= i && i <= max ).ToArray();
            Debug.Assert( arrayBetween1.SequenceEqual( arrayBetween2 ), "!arrayBetween1.SequenceEqual( arrayBetween2 )" );

            foreach ( var t in bptree )
            {
                Console.WriteLine( t );
            }
        }

        private static void VelocityTests()
        {
            const int COUNT = 10_000_000;
            
            //-1.1-
            VelocityTest(() =>
            {
                var bptree = BPlusTreeList< int >.Create( Int32Comparer.Inst, COUNT, 10_000 );
                var sw = Stopwatch.StartNew();
                for ( var i = 1; i <= COUNT; i++ )
                {
                    var t = i; //rnd.Next( 0, COUNT ); //
                    var r = bptree.TryAdd( t );
                }

                return (bptree.GetCount());
            });
            //-1.2-
            VelocityTest(() =>
            {
                var bptree = BPlusTreeList< int >.Create( Int32Comparer.Inst, COUNT, 1_000 );
                var sw = Stopwatch.StartNew();
                for ( var i = COUNT; 0 < i; i-- )
                {
                    var t = i; //rnd.Next( 0, COUNT ); //
                    var r = bptree.TryAdd( t );
                }

                return (bptree.GetCount());
            });

            //-2.1-
            VelocityTest(() =>
            {
                var rnd = new Random();
                var bptree = BPlusTreeList< int >.Create( Int32Comparer.Inst, COUNT, 10_000 );
                var sw = Stopwatch.StartNew();
                for ( var i = 1; i <= COUNT; i++ )
                {
                    var t = rnd.Next( 0, COUNT );
                    var r = bptree.TryAdd( t );
                }

                return (bptree.GetCount());
            });
            
            //-2.2-
            VelocityTest(() =>
            {
                var rnd = new Random();
                var bptree = BPlusTreeList< int >.Create( Int32Comparer.Inst, COUNT, 750 );
                var sw = Stopwatch.StartNew();
                for ( var i = 1; i <= COUNT; i++ )
                {
                    var t = rnd.Next( 0, COUNT );
                    var r = bptree.TryAdd( t );
                }

                return (bptree.GetCount());
            });
            //-2.3-
            VelocityTest(() =>
            {
                var rnd = new Random();
                var bptree = new BPlusTreeList< int >( Int32Comparer.Inst );
                var sw = Stopwatch.StartNew();
                for ( var i = 1; i <= COUNT; i++ )
                {
                    var t = rnd.Next( 0, COUNT );
                    var r = bptree.TryAdd( t );
                }

                return (bptree.GetCount());
            });
        }
        private static void VelocityTest( Func< int > func )
        {
            var sw = Stopwatch.StartNew();
            var count = func();
            sw.Stop();

            Console.WriteLine( "elapsed: " + sw.Elapsed + ", count: " + count );
        }
        private static void VelocityTest()
        {
            const int COUNT = 10000000;
            
            var rnd = new Random();
            var bptree = BPlusTreeList< int >.Create( Int32Comparer.Inst, COUNT, 10_000 );
            var sw = Stopwatch.StartNew();
            for ( var i = 1; i <= COUNT; i++ )
            {
                var t = i; //rnd.Next( 0, COUNT ); //
                var r = bptree.TryAdd( t );
            }
            sw.Stop();

            Console.WriteLine( $"elapsed: {sw.Elapsed}, count: {bptree.GetCount()}" );
        }


        private static void StringTest_1()
        {
            static void action( IBPlusTree< string > bptree )
            {
                bptree.TryAdd( "qwerty" );
                bptree.TryAdd( "qwert" );
                bptree.TryAdd( "qwe" );
                bptree.TryAdd( "qwe" );
                bptree.TryAdd( "qazwwsx" );
                bptree.TryAdd( "xzxzxz" );
                bptree.TryAdd( "zaqwsx" );
                bptree.TryAdd( "XZZZZZYYYY" );
                bptree.TryAdd( "xyzxyz" );

                //var a = bptree.GetValuesBetween( "qwer", "xz", StartsWithStringComparer.Inst ).ToArray();
                //Console.WriteLine( string.Join( ", ", a ) );
            };

            var bptree_lst = new BPlusTreeList< string >( _StringComparer.Inst, 500 );
            var bptree_set = new BPlusTreeSet< string >( _StringComparer.Inst, 500 );

            action( bptree_lst );
            action( bptree_set );

            Debug.Assert( bptree_lst.SequenceEqual( bptree_set ) );

            var bet_1 = bptree_lst.GetValuesBetween( "qwer", "xz", StartsWithStringComparer.Inst );
            var bet_2 = bptree_set.GetValuesBetween( "qwer", "xz", StartsWithStringComparer.Inst );

            Debug.Assert( bet_1.SequenceEqual( bet_2 ) );
        }
        private static void StringTest_2()
        {
            const int COUNT = 5_000_000;

            var rnd = new Random( 7 );
            int  funcGetCharCount() => rnd.Next( 5, 10 + 1 );
            char funcGetChar     () => (char) rnd.Next( '0', '9' + 1 ); //(char) rnd.Next( 'A', 'Z' + 1 );            
            var sw = new Stopwatch();
            var sb = new StringBuilder();

            //-1- fill 'sources'            
            Console.Write( "start fill 'sources'..." );
            var sources = new List< string >( COUNT );
            for ( var i = 1; i <= COUNT; i++ )
            {
                sb.Clear();
                for ( var n = funcGetCharCount(); 0 <= n; n-- )
                {
                    sb.Append( funcGetChar() );
                }
                sources.Add( sb.ToString() );
            }
            Console.WriteLine( $"end, (cnt: {sources.Count})\r\n" );

            //-1-
            void fill_action( IBPlusTree< string > bptree )
            {                
                sw.Restart();
                var add_false_count = 0;
                foreach ( var t in sources )
                {
                    var r = bptree.TryAdd( t );
                    if ( !r )
                    {
                        add_false_count++;
                    }
                }
                sw.Stop();
                Console.WriteLine( $"{bptree.GetType().Name.PadLeft( 15 )}, elapsed: " + sw.Elapsed + ", count: " + bptree.GetCount() + ", add_false_count: " + add_false_count );
            };

            int BLOCK_CAPACITY = 512; // (int) Math.Sqrt( COUNT );
            var bptree_lst = BPlusTreeList< string >.Create( _StringComparer.Inst, COUNT, BLOCK_CAPACITY );
            var bptree_set = new BPlusTreeSet< string >( _StringComparer.Inst, BLOCK_CAPACITY );

            fill_action( bptree_lst );
            fill_action( bptree_set );

            GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );
            GC.WaitForPendingFinalizers();
            GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );

            var find = "12"; // "XZ";
            Debug.Assert( bptree_lst.SequenceEqual( bptree_set ) );
            Debug.Assert( bptree_lst.GetValues( find, StartsWithStringComparer.Inst ).SequenceEqual(
                          bptree_set.GetValues( find, StartsWithStringComparer.Inst ) ) 
                        );
            var min = "4"; // "qwer";
            var max = "5"; //"xz";
            Debug.Assert( bptree_lst.GetValuesBetween( min, max, StartsWithStringComparer.Inst ).SequenceEqual(
                          bptree_set.GetValuesBetween( min, max, StartsWithStringComparer.Inst ) ) 
                        );


            //-2- fill 'sources'
            Console.Write( "\r\nstart fill 'sources'..." );
            sources = new List< string >( COUNT );            
            for ( var i = 1; i <= COUNT; i++ )
            {
                sb.Clear();
                for ( var n = funcGetCharCount(); 0 <= n; n-- )
                {
                    sb.Append( funcGetChar() );
                }
                sources.Add( sb.ToString() );
            }
            Console.WriteLine( $"end, (cnt: {sources.Count})\r\n" );

            //-2-
            void search_action( IBPlusTree< string > bptree )
            {
                sw.Restart();
                var find_count = 0;
                foreach ( var t in sources )
                {
                    var r = bptree.Contains( t );
                    if ( r )
                    {
                        find_count++;
                    }
                }
                sw.Stop();
                Console.WriteLine( $"{bptree.GetType().Name.PadLeft( 15 )}, elapsed: " + sw.Elapsed + " (iteration-count: " + sources.Count + "), find_count: " + find_count );
            };
            search_action( bptree_lst );
            search_action( bptree_set );
        }
        private static void StringTest_3()
        {
            var strings = new[]
            {
                "абрикос", "бабушка", "банан", "баня", "вишня", "говно", "дыня", "елка", "ёлка", "жопа", "залупа", "икра", "кора", "лук", "молоко", "нос", "очки", "пузо", "рис",
            };
            Array.Sort( strings );

            //-1-//
            void fillAction( IBPlusTree< string > bptree )
            {
                foreach ( var s in strings )
                {
                    var r = bptree.TryAdd( s ); Debug.Assert( r );
                }
            };

            var bptree_lst = new BPlusTreeList< string >( _StringComparer.Inst, 7 );
            var bptree_set = new BPlusTreeSet< string >( _StringComparer.Inst, 7 );

            fillAction( bptree_lst );
            fillAction( bptree_set );

            //-2-//
            void compareAction( IBPlusTree< string > bptree )
            {
                var e1 = strings.AsEnumerable().GetEnumerator();
                var e2 = bptree .GetEnumerator();
                for (; ; )
                {
                    var r1 = e1.MoveNext();
                    var r2 = e2.MoveNext();
                    if ( !r1 && !r2 )
                    {
                        break;
                    }
                    Debug.Assert( r1 && r2 );

                    Debug.Assert( e1.Current == e2.Current );
                }
            };

            compareAction( bptree_lst );
            compareAction( bptree_set );

            //-3-//
            void searchAction( IBPlusTree< string > bptree )
            {
                var v = "БА"; // strings[ 1 ].Substring( 0, strings[ 1 ].Length - 1 );
                var val_1 = bptree.GetValues( v, StartsWithStringComparer.Inst ).ToList();
                foreach ( var r in val_1 )
                {
                    Console.WriteLine( r );
                }
                Console.WriteLine();

                var span = strings.AsSpan( 7, 6 );
                var min = span[ 0 ];
                var max = span[ span.Length - 1 ];
                var bet_2 = bptree.GetValuesBetween( min, max ).ToList( span.Length );
                var i = 0;
                foreach ( var r in bet_2 )
                {
                    Debug.Assert( r == span[ i++ ] );
                    Console.WriteLine( r );
                }
                Console.WriteLine();
            };

            searchAction( bptree_lst );
            searchAction( bptree_set );
        }
        public static List< T > ToList< T >( this IEnumerable< T > source, int capacity )
        {
            var lst = new List< T >( capacity );
            lst.AddRange( source );
            return (lst);
        }

        private static void FillTest_1_string()
        {
            const int COUNT = 10_000_000;

            var strings = new List< string >( COUNT );
            for ( int i = 0, len = strings.Capacity; i < len; i++ )
            {
                strings.Add( i.ToString() );
            }
            strings.Reverse();

            void fillAction( ICollection< string > coll )
            {
                var sw = Stopwatch.StartNew();
                var sum = 0;
                for ( var i = 1; i <= COUNT; i++ )
                {
                    var s = strings[ i - 1 ]; //strings[ i % strings.Count ];
                    coll.Add( s );
                    //---sum += coll.Contains( s ) ? 1 : 0;
                }
                if ( coll is List< string > lst )
                {
                    lst.Sort( StringComparer.Ordinal /*_StringComparer.Inst*/ );
                }

                /*
                for ( var i = 1; i <= COUNT; i++ )
                {
                    coll.Remove( i.ToString() );
                }
                */

                sw.Stop();
                Console.WriteLine( $"{coll.GetType().Name.PadLeft( 15 )} => elapsed: {sw.Elapsed}, count: {coll.Count}, ({sum})" );
            };
            //---------------------------------------------------------------//

            int BLOCK_CAPACITY_4_LST = 512; // (int) Math.Sqrt( COUNT );
            var bptree_lst = BPlusTreeList< string >.Create( StringComparer.Ordinal /*_StringComparer.Inst*/, COUNT, BLOCK_CAPACITY_4_LST );

            int BLOCK_CAPACITY_4_SET = 512 >> 1;
            var bptree_set = new BPlusTreeSet< string >( StringComparer.Ordinal /*_StringComparer.Inst*/, BLOCK_CAPACITY_4_SET );


            fillAction( bptree_lst );
            fillAction( bptree_set );

            //---------------------------------------------------------------//

            var lst = new List< string >( COUNT );
            var set = new SortedSet< string >( StringComparer.Ordinal );

            fillAction( lst );
            fillAction( set );
            //---------------------------------------------------------------//
        }
        private static void FillTest_2_int()
        {
            const int COUNT = 20_000_000;

            static void fillAction( ICollection< int > coll )
            {
                var rnd = new Random( 24 );
                var sw = Stopwatch.StartNew();
                //for ( var i = 1; i <= COUNT; i++ )
                //for ( var i = COUNT; 0 < i;  i-- )
                for ( var i = 1; i <= COUNT; i++ )
                {
                    coll.Add( rnd.Next() ); //coll.Add( i );
                }
                if ( coll is List< int > lst )
                {
                    lst.Sort( Int32Comparer.Inst );
                }
                sw.Stop();
                Console.WriteLine( $"{coll.GetType().Name.PadLeft(15)} => elapsed: {sw.Elapsed}, count: {coll.Count}" );
            };

            int BLOCK_CAPACITY_4_LST = 512; // (int) Math.Sqrt( COUNT );
            var bptree_lst = BPlusTreeList< int >.Create( Int32Comparer.Inst, BLOCK_CAPACITY_4_LST, COUNT );

            int BLOCK_CAPACITY_4_SET = 512;//>> 1;
            var bptree_set = new BPlusTreeSet< int >( Int32Comparer.Inst, BLOCK_CAPACITY_4_SET );
                        
            fillAction( bptree_lst );
            fillAction( bptree_set );

            //---------------------------------------------------------------//
            var lst = new List< int >( COUNT );
            var set = new SortedSet< int >( Int32Comparer.Inst );

            fillAction( lst );
            fillAction( set );
            //---------------------------------------------------------------//

            #region commented
            /*
            bptree_lst = null;
            bptree_set = null;

            GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );
            GC.WaitForPendingFinalizers();
            GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );
            */
            #endregion
        }

        private static void Main( string[] args )
        {
            //new BPlusTreeList_vs_List__string( count: 1_000 ).List();

            BenchmarkRunner.Run< BPlusTree__strings >();
            BenchmarkRunner.Run< BPlusTreeSet__strings__MemoryDiagnoser >();
            //BenchmarkRunner.Run< _BPlusTreeList__string >();
            //BenchmarkRunner.Run< _BPlusTreeList__int >();

            //BenchmarkRunner.Run< BPlusTreeList_vs_SortedSet__int >();
            //BenchmarkRunner.Run< BPlusTreeList_vs_SortedSet__string >();
            //BenchmarkRunner.Run< BPlusTreeList_vs_List__int >();

            //Test_1();
            //VelocityTests();

            //FillTest_1_string();
            //FillTest_2_int();

            //StringTest_1();
            //StringTest_2();
            //StringTest_3();            

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\r\n\r\n[.....finita.....]");
            Console.ReadLine();
        }
    }
}
