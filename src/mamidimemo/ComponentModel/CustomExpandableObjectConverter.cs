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
    public class CustomExpandableObjectConverter : ExpandableObjectConverter
    {

        #region コンストラクタ

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public CustomExpandableObjectConverter()
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
                return context.PropertyDescriptor.PropertyType.Name + context.PropertyDescriptor.DisplayName;
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

            var props = base.GetProperties(context, value, attributes);
            return props;
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

    }
}
