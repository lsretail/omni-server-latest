[Code]

var
  CheckPage: TWizardPage;

  CheckPage_SQLCheckBox: TCheckBox; 
  CheckPage_NavSQLCheckBox: TCheckBox;
  CheckPage_IISCheckBox: TCheckBox;
  CheckPage_WSCheckBox: TCheckBox; 

  CheckPage_lblComment: TLabel;
  CheckPage_lblComment1: TLabel;
  CheckPage_lblComment2: TLabel;


procedure CheckCustomForm_Activate(Page: TWizardPage);
begin
  Log('CheckCustomForm_Activate called');
  WizardForm.NextButton.Enabled := True;
end;

procedure OnClickSQL(Sender: TObject);
begin
  if CheckPage_SQLCheckBox.Checked then
  begin
    CheckPage_WSCheckBox.Checked := False;
    CheckPage_WSCheckBox.Enabled := True;
  end
  else
  begin
    CheckPage_WSCheckBox.Checked := False;
    CheckPage_WSCheckBox.Enabled := False;
  end;
end;

procedure OnClickWS(Sender: TObject);
begin
  if CheckPage_WSCheckBox.Checked then
  begin
    CheckPage_NavSQLCheckBox.Checked := False;
    CheckPage_NavSQLCheckBox.Enabled := False;
  end
  else
  begin
    CheckPage_NavSQLCheckBox.Checked := True;
    CheckPage_NavSQLCheckBox.Enabled := True;
  end;
end;

{ IISCustomForm_CreatePage }
function CheckCustomForm_CreatePage(PreviousPageId: Integer): TWizardPage;
begin
  CheckPage := CreateCustomPage(
    PreviousPageId,
    'Installation options.',
    'NOTE: All installation parts must be completed for'#13'a full setup of LS Commerce Service.'
  );
 
  { CheckPage_lblComment }
  CheckPage_lblComment := TLabel.Create(CheckPage);
  with CheckPage_lblComment do
  begin
    Parent := CheckPage.Surface;
    Caption :=  'You can safely run this installation program multiple times.';
    Left := ScaleX(5);
    Top := ScaleY(3);
    Width := ScaleX(400);
    Height := ScaleY(15);
    Enabled := True;
  end;
  
  { CheckPage_lblComment1 }
  CheckPage_lblComment1 := TLabel.Create(CheckPage);
  with CheckPage_lblComment1 do
  begin
    Parent := CheckPage.Surface;
    Caption :=  'Existing SQL objects and Web Application get recreated.';
    Left := ScaleX(5);
    Top := CheckPage_lblComment.Top + CheckPage_lblComment.Height ;
    Width := ScaleX(400);
    Height := ScaleY(15);
    Enabled := True;
  end;

  { CheckPage_SQLCheckBox }
  CheckPage_SQLCheckBox := TCheckBox.Create(CheckPage);
  with CheckPage_SQLCheckBox do
  begin
    Width := CheckPage.SurfaceWidth;
    Height := ScaleY(22);
    Caption := 'Create LS Commerce Service Database';
    Hint := 'Create LS Commerce Database and set the Connection setting to access the Database. Needs SysAdmin right to be able to create the Database.';
    ShowHint := True;
    Checked := True;
    Parent := CheckPage.Surface;
    Top := CheckPage_lblComment.Top + CheckPage_lblComment.Height + 25;
    OnClick := @OnClickSQL;
  end;

  { CheckPage_WSCheckBox }
  CheckPage_WSCheckBox := TCheckBox.Create(CheckPage);
  with CheckPage_WSCheckBox do
  begin
    Width := CheckPage.SurfaceWidth;
    Height := ScaleY(18);
    Caption := 'Use WS Mode for LS Central in SaaS';
    Hint := 'Used with SaaS environment. LS Commerce service will use Web Services to communicate with LS Central in SaaS. No Direct database access to LS Central.';
    ShowHint := True;
    Checked := False;
    Parent := CheckPage.Surface;
    Left := CheckPage_SQLCheckBox.Left + 15
    Top := CheckPage_SQLCheckBox.Top + CheckPage_SQLCheckBox.Height;
    OnClick := @OnClickWS;
  end;

  { CheckPage_NavSQLCheckBox }
  CheckPage_NavSQLCheckBox := TCheckBox.Create(CheckPage);
  with CheckPage_NavSQLCheckBox do
  begin
    Width := CheckPage.SurfaceWidth;
    Height := ScaleY(30);
    Caption := 'Configure LS Nav/LS Central SQL parameters';
    Hint := 'Used with OnPremeses environment. Set the Connection setting to access the LS Central Database.';
    ShowHint := True;
    Checked := True;
    Parent := CheckPage.Surface;
    Top := CheckPage_WSCheckBox.Top + CheckPage_WSCheckBox.Height;
  end;

  { CheckPage_IISCheckBox }
  CheckPage_IISCheckBox := TCheckBox.Create(CheckPage);
  with CheckPage_IISCheckBox do
  begin
    Width := CheckPage.SurfaceWidth;
    Height := ScaleY(30);
    Caption := 'Create LSCommerceService under IIS';
    Hint := 'Add LS Commerce Service to IIS and set the Connection setting to access the LS Central Web Services.';
    ShowHint := True;
    Checked := True;
    Parent := CheckPage.Surface;
    Top := CheckPage_NavSQLCheckBox.Top + CheckPage_NavSQLCheckBox.Height;
  end;

  { CheckPage_lblComment }
  CheckPage_lblComment2 := TLabel.Create(CheckPage);
  with CheckPage_lblComment2 do
  begin
    Parent := CheckPage.Surface;
    Caption :=  'The LSCommerceService is created as a Web Application.'#13'You must have administration rights (sysadmin for SQL server).'#13
         '-->IIS version: ' + GetIISVersionString + ' detected'#13 
         '-->log: ' + expandconstant('{log}') ;
    Left := ScaleX(10);
    Top := CheckPage_IISCheckBox.Top + CheckPage_IISCheckBox.Height + 5;
    Width := CheckPage.SurfaceWidth;
    Height := ScaleY(90);
    Enabled := True;
  end;

  //does not work except from main form
  with CheckPage do
  begin
    OnActivate := @CheckCustomForm_Activate;
  end;

  Result := CheckPage;
end;
