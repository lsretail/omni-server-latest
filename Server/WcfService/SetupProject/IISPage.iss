[Code]

var
  IISPage_lblComment2: TLabel;
  IISPage_lblNavUser: TLabel;
  IISPage_lblNavPwd: TLabel;
  IISPage_lblNavTen: TLabel;
  IISPage_lblNavUrl: TLabel;
  IISPage_lblEComUrl: TLabel;
  IISPage_lblWcfServiceName: TLabel;
  IISPage_lblNavAuthentication : TLabel;
  IISPage_lblWcfSiteName: TLabel;
  IISPage_txtNavUser: TEdit;
  IISPage_txtNavPwd: TPasswordEdit;
  IISPage_txtNavTen: TEdit;
  IISPage_txtNavUrl: TEdit;
  IISPage_txtEComUrl: TEdit;
  IISPage_txtWcfServiceName: TEdit;
  IISPage_txtWcfSiteName : TEdit;
  IISPage_xS2S: TCheckBox; 

 var
  IISPage: TWizardPage;

Procedure IISOnChange(Sender: TObject);
begin                            
  Log('IISOnChange called');
  WizardForm.NextButton.Enabled := False;
  if (Length(IISPage_txtWcfSiteName.Text) > 0) 
      and (Length(IISPage_txtWcfServiceName.Text) > 0) and (Length(IISPage_txtNavUrl.Text) > 0)  
      and (Length(IISPage_txtNavUser.Text) > 0) and (Length(IISPage_txtNavPwd.Text) > 0) then
  begin
    WizardForm.NextButton.Enabled := True;
  end;
end;

procedure S2SOnChange(Sender: TObject);
begin
  Log('S2SOnChange called');
  if IISPage_xS2S.Checked then
  begin
    IISPage_lblNavTen.Visible := True;
    IISPage_txtNavTen.Visible := True;
    IISPage_lblNavUser.Caption := 'Client Id:';
    IISPage_lblNavPwd.Caption := 'Client Secret:';
  end
  Else
  begin
    IISPage_lblNavTen.Visible := False;
    IISPage_txtNavTen.Visible := False;
    IISPage_txtNavTen.Text := '';
    IISPage_lblNavUser.Caption := 'User name:';
    IISPage_lblNavPwd.Caption := 'Password/WebKey:';
  end
end;

procedure IISCustomForm_Activate(Page: TWizardPage) ;
begin
  WizardForm.NextButton.Enabled := False;
  IISPage_xS2S.Checked := False;
  if (Length(IISPage_txtWcfSiteName.Text) > 0) 
      and (Length(IISPage_txtWcfServiceName.Text) > 0) and (Length(IISPage_txtNavUrl.Text) > 0)  
      and (Length(IISPage_txtNavUser.Text) > 0) and (Length(IISPage_txtNavPwd.Text) > 0) then
  begin
    WizardForm.NextButton.Enabled := True;
  end;
end;

{ IISCustomForm_CreatePage }
function IISCustomForm_CreatePage(PreviousPageId: Integer): TWizardPage;
begin
  IISPage := CreateCustomPage(
    PreviousPageId,
    'IIS Web Application Setup',
    'Please enter Web Service Configuration values for IIS and LS Central'
  );
 
  { IISPage_lblWcfSiteName }
  IISPage_lblWcfSiteName := TLabel.Create(IISPage);
  with IISPage_lblWcfSiteName do
  begin
    Parent := IISPage.Surface;
    Caption :=  'Web Site name:';
    Left := ScaleX(15);
    Top := ScaleY(4);
    Width := ScaleX(108);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { IISPage_txtWcfSiteName }
  IISPage_txtWcfSiteName := TEdit.Create(IISPage);
  with IISPage_txtWcfSiteName do
  begin
    Parent := IISPage.Surface;
    Left := ScaleX(120);
    Top := ScaleY(1);
    Width := ScaleX(220);
    Height := ScaleY(21);
    TabOrder := 1;
    Enabled := True;
    ShowHint := True;
    Hint := 'IIS Web Site where LS Commerce Service will be added under. Use -Default Web Site-. A new Web Site does not get created. Recommend leaving the web service name as LSCommerceService.';
  end;

  { IISPage_lblWcfServiceName }
  IISPage_lblWcfServiceName := TLabel.Create(IISPage);
  with IISPage_lblWcfServiceName do
  begin
    Parent := IISPage.Surface;
    Caption :=  'Web Service name:';
    Left := ScaleX(15);
    Top := ScaleY(29);
    Width := ScaleX(108);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { IISPage_txtWcfServiceName }
  IISPage_txtWcfServiceName := TEdit.Create(IISPage);
  with IISPage_txtWcfServiceName do
  begin
    Parent := IISPage.Surface;
    Left := ScaleX(120);
    Top := ScaleY(26);
    Width := ScaleX(220);
    Height := ScaleY(21);
    TabOrder := 2;
    Enabled := True;
    ShowHint := True;
    Hint := 'Name used to Create IIS entry. If running more than 1 instance of LS Commerce Service, give each instance a different Name.';
  end;

  { IISPage_lblEComUrl }
  IISPage_lblEComUrl := TLabel.Create(IISPage);
  with IISPage_lblEComUrl do
  begin
    Parent := IISPage.Surface;
    Caption :=  'ECom webhook URL:';
    Left := ScaleX(15);
    Top := ScaleY(56);
    Width := ScaleX(108);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { IISPage_txtEComUrl }
  IISPage_txtEComUrl := TEdit.Create(IISPage);
  with IISPage_txtEComUrl do
  begin
    Parent := IISPage.Surface;
    Left := ScaleX(120);
    Top := ScaleY(53);
    Width := ScaleX(280);
    Height := ScaleY(21);
    TabOrder := 3;
    Enabled := True;
    OnChange := @IISOnChange;
    ShowHint := True;
    Hint := 'Magento WebHook URL to send Order status updates back to Magento from LS Central. Demo in URI field will always return back to LS Central if there is no Magento running.';
  end; 

  { IISPage_lblNavUrl }
  IISPage_lblNavUrl := TLabel.Create(IISPage);
  with IISPage_lblNavUrl do
  begin
    Parent := IISPage.Surface;
    Caption :=  'LSCentral WS URL:';
    Left := ScaleX(15);
    Top := ScaleY(81);
    Width := ScaleX(108);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { IISPage_txtNavUrl }
  IISPage_txtNavUrl := TEdit.Create(IISPage);
  with IISPage_txtNavUrl do
  begin
    Parent := IISPage.Surface;
    Left := ScaleX(120);
    Top := ScaleY(78);
    Width := ScaleX(280);
    Height := ScaleY(21);
    TabOrder := 4;
    Enabled := True;
    OnChange := @IISOnChange;
    ShowHint := True;
    Hint := 'LS Central URI to RetailWebServices Web Service. Can be found under Web Services in LS Central.';
  end; 

  { IISPage_lblNavAuthentication }
  IISPage_lblNavAuthentication := TLabel.Create(IISPage);
  with IISPage_lblNavAuthentication do
  begin
    Parent := IISPage.Surface;
    Caption :=  'LS Central Web Services Authentication.';
    Left := ScaleX(15);
    Top := ScaleY(107);
    Width := ScaleX(230);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { IISPage_xS2S }
  IISPage_xS2S := TCheckBox.Create(SQLPage);
  with IISPage_xS2S do
  begin
    Parent := IISPage.Surface;
    Left := ScaleX(250);
    Top := ScaleY(104);
    Width := ScaleX(380);
    Height := ScaleY(21);
    Caption := 'Use S2S oAuth';
    Checked := False;
    TabOrder := 5;
    ShowHint := True;
    OnClick := @S2SOnChange;
    Hint := 'Use oAuth S2S to log into LS Central in SaaS.';
  end;

  { IISPage_lblNavUser }
  IISPage_lblNavUser := TLabel.Create(IISPage);
  with IISPage_lblNavUser do
  begin
    Parent := IISPage.Surface;
    Caption :=  'User name:';
    Left := ScaleX(15);
    Top := ScaleY(132);
    Width := ScaleX(108);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { IISPage_txtNavUser }
  IISPage_txtNavUser := TEdit.Create(IISPage);
  with IISPage_txtNavUser do
  begin
    Parent := IISPage.Surface;
    Left := ScaleX(120);
    Top := ScaleY(129);
    Width := ScaleX(220);
    Height := ScaleY(21);
    TabOrder := 6;
    Enabled := True;
    OnChange := @IISOnChange;
    ShowHint := True;
    Hint := 'User with access to LS Central Web Service. Active Directory, workgroup or local computer user. Can be DOMAIN\UserName';
  end;  

  { IISPage_lblNavPwd }
  IISPage_lblNavPwd := TLabel.Create(IISPage);
  with IISPage_lblNavPwd do
  begin
    Parent := IISPage.Surface;
    Caption :=  'Password/WebKey:';
    Left := ScaleX(15);
    Top := ScaleY(157);
    Width := ScaleX(108);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { IISPage_txtNavPwd }
  IISPage_txtNavPwd := TPasswordEdit.Create(IISPage);
  with IISPage_txtNavPwd do
  begin
    Parent := IISPage.Surface;
    Left := ScaleX(120);
    Top := ScaleY(154);
    Width := ScaleX(220);
    Height := ScaleY(21);
    TabOrder := 7;
    Enabled := True;
    OnChange := @IISOnChange;
    ShowHint := True;
    Hint := 'Password for the User to access LS Central Web Service. For LS Central in SaaS use Webkey here.';
  end;  

  { IISPage_lblNavTen }
  IISPage_lblNavTen := TLabel.Create(IISPage);
  with IISPage_lblNavTen do
  begin
    Parent := IISPage.Surface;
    Caption :=  'Tenant Id:';
    Left := ScaleX(15);
    Top := ScaleY(182);
    Width := ScaleX(108);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { IISPage_txtNavTen }
  IISPage_txtNavTen := TEdit.Create(IISPage);
  with IISPage_txtNavTen do
  begin
    Parent := IISPage.Surface;
    Left := ScaleX(120);
    Top := ScaleY(179);
    Width := ScaleX(220);
    Height := ScaleY(21);
    TabOrder := 8;
    Enabled := True;
    ShowHint := True;
    Hint := 'LS Central Tenant Id.';
  end;  

  IISPage_lblComment2 := TLabel.Create(IISPage);
  with IISPage_lblComment2 do
  begin
    Parent := IISPage.Surface;
    Caption := 'All the configuration values will be stored in AppSettings.Config file';
    Left := ScaleX(15);
    Top := ScaleY(217);
    Width := ScaleX(330);
    Height := ScaleY(200);
    Enabled := True;
  end;

  //does not work except from main form
  with IISPage do
  begin
    OnActivate := @IISCustomForm_Activate;
  end;

  Result := IISPage;
end;
