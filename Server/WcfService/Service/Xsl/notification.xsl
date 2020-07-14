<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<xsl:template match="/Order">
<xsl:if test="Status = 'Ready'">
Dear <xsl:value-of select="FirstName"/>&nbsp;<xsl:value-of select="LastName"/><br/>
Your order number:&nbsp;&nbsp;<xsl:value-of select="DocID"/> <br/>
is ready for collection until:&nbsp; <xsl:value-of select="CollectTimeLimit"/> <br/>
in store:&nbsp;&nbsp;<xsl:value-of select="StoreName"/>,&nbsp;<xsl:value-of select="StoreAddress"/>
<br/><br/>
<xsl:choose>
  <xsl:when test="OrderLinesToCollect/Line !='' ">  
    Items in your order:
    <xsl:for-each select="OrderLinesToCollect/Line">
      <br/>
      <xsl:value-of select="ItemDescription"/>.    
        <xsl:if test="VariantCode != ''">
          &nbsp;(<xsl:value-of select="VariantCode"/>)
        </xsl:if>
      <br/>Qty: <xsl:value-of select="Quantity"/><br/>Price:&nbsp;<xsl:value-of select='format-number(Amount,"###,###.00")'/>
     </xsl:for-each>
  </xsl:when>
</xsl:choose>
  
  <br/>
Total amount: &nbsp;<xsl:value-of select='format-number(EstimatedTotalAmount,"###,###.00")'/>
 
  <br/><br/>

  <xsl:choose>
    <xsl:when test="OrderLinesOutOfStock/Line !='' ">
        These items you ordered are out of stock and are not available at this time.
      <xsl:for-each select="OrderLinesOutOfStock/Line">
        <br/>
        <xsl:value-of select="ItemDescription"/>.
          <xsl:if test="VariantCode != ''">
            &nbsp;(<xsl:value-of select="VariantCode"/>)
          </xsl:if>
        <br/>Qty: <xsl:value-of select="Quantity"/><br/>Price:&nbsp;<xsl:value-of select='format-number(Amount,"###,###.00")'/>
    </xsl:for-each>
    </xsl:when>
</xsl:choose>
  <br/>
  Loyalty card used: <xsl:value-of select="MemberCardNo"/>
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
    <xsl:for-each select="OrderLinesToCollect/Line">
      <br/>
      <xsl:value-of select="ItemDescription"/>.    
        <xsl:if test="VariantCode != ''">
          &nbsp;(<xsl:value-of select="VariantCode"/>)
        </xsl:if>
      <br/>Qty: <xsl:value-of select="Quantity"/><br/>Price:&nbsp;<xsl:value-of select='format-number(Amount,"###,###.00")'/>
     </xsl:for-each>
  </xsl:when>
</xsl:choose>
  <xsl:choose>
    <xsl:when test="OrderLinesOutOfStock/Line !='' ">
      <xsl:for-each select="OrderLinesOutOfStock/Line">
        <br/>
        <xsl:value-of select="ItemDescription"/>.
          <xsl:if test="VariantCode != ''">
            &nbsp;(<xsl:value-of select="VariantCode"/>)
          </xsl:if>
        <br/>Qty: <xsl:value-of select="Quantity"/><br/>Price:&nbsp;<xsl:value-of select='format-number(Amount,"###,###.00")'/>
    </xsl:for-each>
    </xsl:when>
</xsl:choose>

</xsl:if>
  
  <br/><br/>Disclaimer:&nbsp;<xsl:value-of select="Disclaimer"/>
</xsl:template>
</xsl:stylesheet>
 