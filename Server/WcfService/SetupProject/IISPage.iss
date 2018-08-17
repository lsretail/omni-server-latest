[Code]

var
  IISPage_lblComment: TLabel;
  IISPage_lblComment1: TLabel;
  IISPage_lblComment2: TLabel;
  IISPage_lblNavUser: TLabel;
  IISPage_lblNavPwd: TLabel;
  IISPage_lblNavUrl: TLabel;
  IISPage_lblWcfServiceName: TLabel;
  IISPage_lblNavAuthentication : TLabel;
  IISPage_lblWcfSiteName: TLabel;
  IISPage_txtNavUser: TEdit;
  IISPage_txtNavPwd: TPasswordEdit;
  IISPage_txtNavUrl: TEdit;
  IISPage_txtWcfServiceName: TEdit;
  IISPage_txtWcfSiteName : TEdit;

 var
  IISPage: TWizardPage;


Procedure IISOnChange (Sender: TObject);
begin                            
    WizardForm.NextButton.Enabled := False;
    if (Length(IISPage_txtWcfSiteName.Text) > 0)
      and (Length(IISPage_txtWcfServiceName.Text) > 0) and (Length(IISPage_txtNavUrl.Text) > 0)  
      and (Length(IISPage_txtNavUser.Text) > 0) and (Length(IISPage_txtNavPwd.Text) > 0) then
    begin
      WizardForm.NextButton.Enabled := True;
    end;
end;
              
procedure IISCustomForm_Activate(Page: TWizardPage) ;
begin
    Log('IISCustomForm_Activate called');
    WizardForm.NextButton.Enabled := False;
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
    'Please enter Web Servie Configuration values for IIS'
  );
 
  { IISPage_lblComment }
  IISPage_lblComment := TLabel.Create(IISPage);
  with IISPage_lblComment do
  begin
    Parent := IISPage.Surface;
    Caption :=  'Recommend leaving the web service name as LSOmniService';
    Left := ScaleX(15);
    Top := ScaleY(1);
    Width := ScaleX(350);
    Height := ScaleY(13);
    Enabled := True;
  end;

  { IISPage_lblWcfSiteName }
  IISPage_lblWcfSiteName := TLabel.Create(IISPage);
  with IISPage_lblWcfSiteName do
  begin
    Parent := IISPage.Surface;
    Caption :=  'Web Site name:';
    Left := ScaleX(15);
    Top := IISPage_lblComment.Top + IISPage_lblComment.Height + 7;
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
    Top := IISPage_lblComment.Top + IISPage_lblComment.Height + 4;
    Width := ScaleX(220);
    Height := ScaleY(21);
    TabOrder := 1;
    Enabled := True;
    ShowHint := True;
    Hint     := 'Use -Default Web Site- A new Web Site does not get created';
    //OnChange := @IISOnChange;
  end;

  { IISPage_lblWcfServiceName }
  IISPage_lblWcfServiceName := TLabel.Create(IISPage);
  with IISPage_lblWcfServiceName do
  begin
    Parent := IISPage.Surface;
    Caption :=  'Web Service name:';
    Left := ScaleX(15);
    Top := IISPage_txtWcfSiteName.Top + IISPage_txtWcfSiteName.Height + 5;
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
    Top := IISPage_txtWcfSiteName.Top + IISPage_txtWcfSiteName.Height + 2;
    Width := ScaleX(220);
    Height := ScaleY(21);
    TabOrder := 2;
    Enabled := True;
    //OnChange := @IISOnChange;
  end;

  { IISPage_lblComment1 }
  IISPage_lblComment1 := TLabel.Create(IISPage);
  with IISPage_lblComment1 do
  begin
    Parent := IISPage.Surface;
    Caption :=  'NAV web service configuration values. Used to connect to NAV ws';
    Left := ScaleX(15);
    Top := IISPage_txtWcfServiceName.Top + IISPage_txtWcfServiceName.Height + 8;
    Width := ScaleX(350);
    Height := ScaleY(13);
    Enabled := True;
  end;

  { IISPage_lblNavUrl }
  IISPage_lblNavUrl := TLabel.Create(IISPage);
  with IISPage_lblNavUrl do
  begin
    Parent := IISPage.Surface;
    Caption :=  'NAV web service Url:';
    Left := ScaleX(15);
    Top :=IISPage_lblComment1.Top + IISPage_lblComment1.Height + 7;
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
    Top := IISPage_lblComment1.Top + IISPage_lblComment1.Height + 4;
    Width := ScaleX(280);
    Height := ScaleY(21);
    TabOrder := 4;
    Enabled := True;
  end; 

  { IISPage_lblNavAuthentication }
  IISPage_lblNavAuthentication := TLabel.Create(IISPage);
  with IISPage_lblNavAuthentication do
  begin
    Parent := IISPage.Surface;
    Caption :=  'NAV Web Services Authentication. Uses Windows Credential type.';
    Left := ScaleX(15);
    Top :=  IISPage_txtNavUrl.Top + IISPage_txtNavUrl.Height + 8;
    Width := ScaleX(330);
    Height := ScaleY(13);
    Enabled := True;

  end;

  { IISPage_lblNavUser }
  IISPage_lblNavUser := TLabel.Create(IISPage);
  with IISPage_lblNavUser do
  begin
    Parent := IISPage.Surface;
    Caption :=  'User name:';
    Left := ScaleX(30);
    Top := IISPage_lblNavAuthentication.Top + IISPage_lblNavAuthentication.Height + 7;
    Width := ScaleX(108);
    Height := ScaleY(13);
    Enabled := True;
    ShowHint := True;
    Hint     := 'Active Directory, local workgroup, or the local computer users. Can be DOMAIN\UserName';
  end;
  { IISPage_txtNavUser }
  IISPage_txtNavUser := TEdit.Create(IISPage);
  with IISPage_txtNavUser do
  begin
    Parent := IISPage.Surface;
    Left := ScaleX(120);
    Top := IISPage_lblNavAuthentication.Top + IISPage_lblNavAuthentication.Height + 4;
    Width := ScaleX(220);
    Height := ScaleY(21);
    TabOrder := 5;
    Enabled := True;
    ShowHint := True;
    Hint     := 'Active Directory, local workgroup, or the local computer user. Can be DOMAIN\UserName';
  end;  

  { IISPage_lblNavPwd }
  IISPage_lblNavPwd := TLabel.Create(IISPage);
  with IISPage_lblNavPwd do
  begin
    Parent := IISPage.Surface;
    Caption :=  'Password:';
    Left := ScaleX(30);
    Top := IISPage_txtNavUser.Top + IISPage_txtNavUser.Height + 5;
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
    Top := IISPage_txtNavUser.Top + IISPage_txtNavUser.Height + 2;
    Width := ScaleX(220);
    Height := ScaleY(21);
    TabOrder := 6;
    Enabled := True;
    OnChange := @IISOnChange;
  end;  

  IISPage_lblComment2 := TLabel.Create(IISPage);
  with IISPage_lblComment2 do
  begin
    Parent := IISPage.Surface;
    Caption := 'All config values are stored in AppSettings.Config file';
    Left := ScaleX(15);
    Top := IISPage_txtNavPwd.Top + IISPage_txtNavPwd.Height + 8;
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


