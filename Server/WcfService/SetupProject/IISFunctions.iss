[Code]
function VariantChangeType(out Dest: Variant; Source: Variant; Flags, vt: Word): HRESULT; external 'VariantChangeType@oleaut32.dll stdcall';

function VarToDisp(Source: Variant): Variant;
begin
  Result := Unassigned;
  OleCheck(VariantChangeType(Result, Source, 0, varDispatch));
end; 

// creates a web application under Default Web Site and a application poo called  webAppName+'Pool'
function IISCreateWebApplication(webSiteName: string; webAppName: string; physicalPath: string; CmdMode: Boolean): Boolean;
var
  appPool : Variant;
  adminManager: Variant;
  sitesSection,sitesCollection : Variant;
  i, siteElementPos: Integer;
  Properties,Item :Variant;

  applicationPoolsSection, applicationPoolsCollection: Variant;
  addElement : Variant;
  found : Boolean;

  siteElement,siteCollection,applicationElement : Variant;
  applicationCollection,virtualDirectoryElement : Variant;
  webAppExists : Boolean;

begin
  Result := True;
  webAppExists := False;
  //webAppName is the POSService,  physicalPath is c:\LS Retail\LSOmni ..
  webAppName := Trim(webAppName);
  webSiteName := Trim(webSiteName);
  physicalPath := Trim(physicalPath + '\' + webAppName);
  appPool := webAppName+'Pool';
  webAppName := '/' + webAppName; //  "/POSService"

  if (Length(webSiteName) = 0) then
  begin
     webSiteName := 'Default Web Site';
  end;

  try
    adminManager := CreateOleObject('Microsoft.ApplicationHost.WritableAdminManager'); 
  except
    ErrorMsg('Please install Microsoft IIS first.', CmdMode);
    Result := False;
    Exit;
  end;

  try
    log('IISCreateWebApplication() webAppName: ' + webAppName + ' physicalPath:' + physicalPath);

    //from http://stackoverflow.com/questions/17299094/could-not-convert-variant-of-type-unknown-into-type-dispatch
    // http://www.iis.net/configreference/system.applicationhost/sites/site/application
    adminManager.CommitPath := 'MACHINE/WEBROOT/APPHOST';
    sitesSection := VarToDisp(adminManager.GetAdminSection('system.applicationHost/sites', 'MACHINE/WEBROOT/APPHOST'));
    sitesCollection := VarToDisp(sitesSection.Collection);
    siteElementPos := 0;    //Default Web Site is usually 0 but we look it up

    //look for the siteElementPos for the web site
    for i := 0 to sitesCollection.Count-1 do 
	begin
      Item := VarToDisp(sitesCollection.Item(i));
      Properties := VarToDisp(Item.Properties);
      if (Lowercase(VarToDisp(Properties.Item('name')).Value) = Lowercase(webSiteName)) then
        siteElementPos := i;   //usually Default Web Site = 0 
    end;
  
    siteElement := VarToDisp(sitesCollection.Item(siteElementPos));
    siteCollection := VarToDisp(siteElement.Collection);

    for i := 0 to siteCollection.Count-1 do 
	begin
      Item := VarToDisp(siteCollection.Item(i));
      Properties := VarToDisp(Item.Properties);
      if (Lowercase(VarToDisp(Properties.Item('path')).Value) = Lowercase(webAppName)) then
      begin
        siteCollection.DeleteElement(i);
        webAppExists := True;     
      end;
    end;

    if (webAppExists) then
      siteCollection := VarToDisp(siteElement.Collection);

    //web app does not exist, so create it
    applicationElement := VarToDisp(siteCollection.CreateNewElement('application'));
    VarToDisp(VarToDisp(applicationElement.Properties).Item('path')).Value := webAppName;
    VarToDisp(VarToDisp(applicationElement.Properties).Item('applicationPool')).Value := appPool;  
    applicationCollection := VarToDisp(applicationElement.Collection);
    virtualDirectoryElement := VarToDisp(applicationCollection.CreateNewElement('virtualDirectory'));

    // set the physical path 
    VarToDisp(VarToDisp(virtualDirectoryElement.Properties).Item('path')).Value := '/';
    VarToDisp(VarToDisp(virtualDirectoryElement.Properties).Item('physicalPath')).Value := physicalPath;
    applicationCollection.AddElement(virtualDirectoryElement);
    siteCollection.AddElement(applicationElement);

    //now create the application pool
    applicationPoolsSection := VarToDisp(adminManager.GetAdminSection('system.applicationHost/applicationPools','MACHINE/WEBROOT/APPHOST'));
    applicationPoolsCollection := VarToDisp(applicationPoolsSection.Collection);

    // check if app pool already exists before adding it
    found := False;
    for i := 0 to applicationPoolsCollection.Count-1 do 
    begin
      Item := VarToDisp(applicationPoolsCollection.Item(i));
      Properties := VarToDisp(Item.Properties);
      if (Lowercase(VarToDisp(Properties.Item('name')).Value) = Lowercase(appPool)) then
      begin
        applicationPoolsCollection.DeleteElement(i);
        found := True;
      end;
    end;
    if found then
    begin
      applicationPoolsCollection := VarToDisp(applicationPoolsSection.Collection);
    end;
    
	//http://www.iis.net/configreference/system.applicationhost/applicationpools/applicationpooldefaults
    addElement := VarToDisp(applicationPoolsCollection.CreateNewElement('add'));
    VarToDisp(VarToDisp(addElement.Properties).Item('name')).Value := appPool;
    VarToDisp(VarToDisp(addElement.Properties).Item('autoStart')).Value := True;
    VarToDisp(VarToDisp(addElement.Properties).Item('managedRuntimeVersion')).Value := 'v4.0';
    VarToDisp(VarToDisp(addElement.Properties).Item('managedPipelineMode')).Value := 'Integrated';
    applicationPoolsCollection.AddElement(addElement);

    adminManager.CommitChanges();
  except
    //RaiseException('Please install Microsoft IIS first.'#13#13'(Error ''' + GetExceptionMessage + ''' occurred)');
    ErrorMsg('Failed to create web application.', CmdMode);
    Result := False;
    Exit;
  end;
end;
