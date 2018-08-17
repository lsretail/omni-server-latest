<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:template match="/Order">
<html>
<body>
<xsl:if test="Status = 'Ready'">
Dear <xsl:value-of select="FirstName"/>&nbsp;<xsl:value-of select="LastName"/><br/>
Your order number:&nbsp;&nbsp;<xsl:value-of select="DocID"/> 
is ready for collection until:&nbsp; <xsl:value-of select="CollectTimeLimit"/> 
in store:&nbsp;&nbsp;<xsl:value-of select="StoreName"/>,&nbsp;<xsl:value-of select="StoreAddress"/>
<br/><br/>

  <xsl:choose>
    <xsl:when test="OrderLinesToCollect/Line !='' ">
      Items in your order:
      
      <table border="0">
      <tr bgcolor="lightgrey">
        <th style="text-align:center">Product</th>
        <th style="text-align:center">Qty</th>
        <th style="text-align:right">Price</th>
      </tr>
      <xsl:for-each select="OrderLinesToCollect/Line">
      <tr>
        <td width="200pt" style="text-align:left">
          <xsl:value-of select="ItemDescription"/>.
          <xsl:if test="VariantCode != ''">
            &nbsp;(<xsl:value-of select="VariantCode"/>)
          </xsl:if>
	      </td>
	      <td style="text-align:center"><xsl:value-of select="Quantity"/></td>
        <td style="text-align:right"><xsl:value-of select='format-number(Amount,"###,###.00")'/></td>
      </tr>
      </xsl:for-each>
      <tr>
        <td style="text-align:right"><b>Total:&nbsp;</b></td>
	      <td></td>
        <td style="text-align:right">
          <b><xsl:value-of select='format-number(EstimatedTotalAmount,"###,###.00")'/></b>
        </td>
      </tr>
    </table>
    </xsl:when>
  </xsl:choose>

  <br/>

  <xsl:choose>
    <xsl:when test="OrderLinesOutOfStock/Line !='' ">
      These items you ordered are out of stock and are not available at this time.
      <table border="0">
        <tr bgcolor="lightgrey">
          <th style="text-align:center">Product</th>
          <th style="text-align:center">Qty</th>
          <th style="text-align:right">Price</th>
        </tr>
        <xsl:for-each select="OrderLinesOutOfStock/Line">
          <tr>
            <td width="200pt" style="text-align:left">
              <xsl:value-of select="ItemDescription"/>.
              <xsl:if test="VariantCode != ''">
                &nbsp;(<xsl:value-of select="VariantCode"/>)
              </xsl:if>
            </td>
            <td style="text-align:center">
              <xsl:value-of select="Quantity"/>
            </td>
            <td style="text-align:right">
              <xsl:value-of select='format-number(Amount,"###,###.00")'/>
            </td>
          </tr>
      </xsl:for-each>
      </table>
    </xsl:when>
  </xsl:choose>
  <br/>
  Loyalty card used: <xsl:value-of select="MemberCardNo"/>
  <br/>
<img src="cid:"/>
</xsl:if>
<!-- endif order = canceled  -->
  <xsl:if test="Status = 'Canceled'">
    Dear <xsl:value-of select="FirstName"/>&nbsp;<xsl:value-of select="LastName"/><br/>
    Your order number:&nbsp;&nbsp;<xsl:value-of select="DocID"/> &nbsp;&nbsp;
    in store:&nbsp;&nbsp;<xsl:value-of select="StoreToCollect"/>
    has been cancelled.
<br/><br/>
Items in your order:
  <xsl:choose>
    <xsl:when test="OrderLinesToCollect/Line !='' ">
      <table border="0">
      <tr bgcolor="lightgrey">
        <th style="text-align:center">Product</th>
        <th style="text-align:center">Qty</th>
        <th style="text-align:right">Price</th>
      </tr>
      <xsl:for-each select="OrderLinesToCollect/Line">
      <tr>
        <td width="200pt" style="text-align:left">
          <xsl:value-of select="ItemDescription"/>.
          <xsl:if test="VariantCode != ''">
            &nbsp;(<xsl:value-of select="VariantCode"/>)
          </xsl:if>
	      </td>
	      <td style="text-align:center"><xsl:value-of select="Quantity"/></td>
        <td style="text-align:right"><xsl:value-of select='format-number(Amount,"###,###.00")'/></td>
      </tr>
      </xsl:for-each>
      <tr>
        <td style="text-align:right"><b>Total:&nbsp;</b></td>
	      <td></td>
        <td style="text-align:right">
          <b><xsl:value-of select='format-number(EstimatedTotalAmount,"###,###.00")'/></b>
        </td>
      </tr>
    </table>
    </xsl:when>
  </xsl:choose>
  <xsl:choose>
    <xsl:when test="OrderLinesOutOfStock/Line !='' ">
      <table border="0">
        <tr bgcolor="lightgrey">
          <th style="text-align:center">Product</th>
          <th style="text-align:center">Qty</th>
          <th style="text-align:right">Price</th>
        </tr>s
        <xsl:for-each select="OrderLinesOutOfStock/Line">
          <tr>
            <td width="200pt" style="text-align:left">
              <xsl:value-of select="ItemDescription"/>.
              <xsl:if test="VariantCode != ''">
                &nbsp;(<xsl:value-of select="VariantCode"/>)
              </xsl:if>
            </td>
            <td style="text-align:center">
              <xsl:value-of select="Quantity"/>
            </td>
            <td style="text-align:right">
              <xsl:value-of select='format-number(Amount,"###,###.00")'/>
            </td>
          </tr>
      </xsl:for-each>
      </table>
    </xsl:when>
  </xsl:choose>
  </xsl:if>  
<br/><br/>Disclaimer:&nbsp;<xsl:value-of select="Disclaimer"/> 
<br/>
  </body>
  </html>
</xsl:template>
</xsl:stylesheet>
