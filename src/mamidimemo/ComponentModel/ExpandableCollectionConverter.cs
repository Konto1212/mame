/*
 * © 2007, 2010, 2011 Renesas Electronics Corporation
 * RENESAS ELECTRONICS CONFIDENTIAL AND PROPRIETARY
 * This program must be used solely for the purpose for which
 * it was furnished by Renesas Electronics Corporation. No part of this
 * program may be reproduced or disclosed to others, in any
 * form, without the prior written permission of Renesas Electronics
 * Corporation.
 * 
 * $Id: DialogEditOnlyExpandableCollectionConverter.cs 8029 2011-01-26 06:29:40Z kenichi.kashima $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Globalization;

namespace zanac.MAmidiMEmo.ComponentModel
{
    /// <summary>
    /// </summary>
    public class CustomCollectionConverter : CollectionConverter
    {

        #region コンストラクタ

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public CustomCollectionConverter()
        {
            //なにもしない
        }

        #endregion //コンストラクタ end

        #region メソッド

        /// <summary>
        /// 指定したコンテキストとカルチャ情報を使用して、指定した値オブジェクトを、指定した型に変換します。
        /// </summary>
        /// <param name="context">書式指定コンテキストを提供する <see cref="ITypeDescriptorContext"/> 。 </param>
        /// <param name="culture"><see cref="CultureInfo"/> オブジェクト。 null 参照 (Visual Basic では Nothing) が渡された場合は、現在のカルチャが使用されます。 </param>
        /// <param name="value">変換対象の <see cref="Object"/> 。 </param>
        /// <param name="destinationType"></param>
        /// <returns>value パラメータの変換後の <see cref="Type"/> 。 </returns>
        /// <exception cref="ArgumentNullException">引数がnull</exception>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");

            if (destinationType == typeof(string))
            {
                ICollection c = value as ICollection;
                if (c != null)
                    return context.PropertyDescriptor.DisplayName + "[" + c.Count + "]";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// 指定したコンテキストと属性を使用して、value パラメータで指定されたArrayList型のプロパティのコレクションを返します。
        /// </summary>
        /// <param name="context">書式指定コンテキストを提供する <see cref="ITypeDescriptorContext"/> 。</param>
        /// <param name="value">プロパティを取得する対象となる配列の型を指定する <see cref="Object"/> 。またはnull</param>
        /// <param name="attributes">フィルタとして使用される、 <see cref="Attribute"/> 型の配列。またはnull。この引数は使用されていません。</param>
        /// <returns>指定されたデータ型に対して公開されているプロパティを格納している <see cref="PropertyDescriptorCollection"/> 。
        /// コレクションにプロパティが格納されていない場合は null 参照</returns>
        /// <exception cref="ArgumentNullException">引数がnull</exception>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            PropertyDescriptor[] array = null;
            ICollection list = value as ICollection;
            if (list != null)
            {
                array = new PropertyDescriptor[list.Count];
                Type type = typeof(ICollection);
                int i = 0;
                foreach (object o in list)
                {
                    string name = string.Format(CultureInfo.InvariantCulture,
                        "[{0}]", i.ToString("d" + list.Count.ToString
                        (NumberFormatInfo.InvariantInfo).Length, null));
                    CollectionPropertyDescriptor cpd = new CollectionPropertyDescriptor(context, type, name, o.GetType(), i);
                    array[i] = cpd;
                    i++;
                }
            }
            return new PropertyDescriptorCollection(array);
        }

        /// <summary>
        /// オブジェクトがプロパティをサポートしているかどうかを示す値を返します。
        /// </summary>
        /// <param name="context">書式指定コンテキストを提供する <see cref="ITypeDescriptorContext"/> 。</param>
        /// <returns>常にtrue</returns>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        #endregion //メソッド end

        #region 内部クラス

        /// <summary>
        /// IListのプロパティデスクリプタ クラス
        /// </summary>
        private class CollectionPropertyDescriptor : SimplePropertyDescriptor
        {

            #region フィールド/プロパティ

            /// <summary>
            /// 書式指定コンテキストを提供する <see cref="ITypeDescriptorContext"/> 。
            /// </summary>
            private ITypeDescriptorContext context;

            /// <summary>
            /// IListにおけるインデックス位置
            /// </summary>
            private int index;

            /// <summary>
            /// 読み込み専用かどうか
            /// </summary>
            /// <remarks>
            /// 常にtrueを返す。
            /// </remarks>
            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            #endregion

            #region コンストラクタ

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="context">書式指定コンテキストを提供する <see cref="ITypeDescriptorContext"/> 。</param>
            /// <param name="index">IListに対するプロパティのインデックス</param>
            /// <param name="componentType">プロパティ記述子が関連付けられているコンポーネントの型を表す Type 。 </param>
            /// <param name="name">プロパティの名前。</param>
            /// <param name="elementType">プロパティのデータ型を表す <see cref="Type"/> 。 </param>
            public CollectionPropertyDescriptor(ITypeDescriptorContext context, Type componentType, string name, Type elementType, int index)
                : base(componentType, name, elementType)
            {
                this.context = context;
                this.index = index;
            }

            #endregion

            #region インスタンス メソッド

            /// <summary>
            /// コンポーネントのプロパティの現在の値を取得します。
            /// </summary>
            /// <param name="component">値の取得対象であるプロパティを持つコンポーネント。</param>
            /// <returns>現在の値</returns>
            public override object GetValue(object component)
            {
                ICollection c = component as ICollection;
                if (c != null)
                {
                    if (c.Count > index)
                    {
                        int i = 0;
                        foreach (object o in c)
                        {
                            if (i == index)
                                return o;
                            i++;
                        }
                    }
                }
                return null;
            }
            
            /// <summary>
            /// コンポーネントがIListを実装する場合、指定の値に設定します。
            /// </summary>
            /// <param name="component">設定する対象のプロパティ値を持つコンポーネント。 </param>
            /// <param name="value">新しい値。</param>
            public override void SetValue(object component, object value)
            {
                IList c = component as IList;
                if(c != null)
                    c[index]  = value;
            }

            /// <summary>
            /// プロパティの説明を取得します。
            /// ※このプロパティが無いと説明が表示されません。
            /// </summary>
            public override string Description
            {
                get
                {
                    return context.PropertyDescriptor.Description;
                }
            }

            #endregion

        }

        #endregion //内部クラス end
    }
}
