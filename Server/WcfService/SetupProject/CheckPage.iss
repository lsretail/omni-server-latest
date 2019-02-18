[Code]

var
  CheckPage_SQLCheckBox: TCheckBox; 
  CheckPage_NavSQLCheckBox: TCheckBox;
  CheckPage_IISCheckBox: TCheckBox;

  CheckPage_lblComment: TLabel;
  CheckPage_lblComment1: TLabel;
  CheckPage_lblComment2: TLabel;
 var
  CheckPage: TWizardPage;
              

procedure CheckCustomForm_Activate(Page: TWizardPage) ;
begin
    Log('CheckCustomForm_Activate called');
    WizardForm.NextButton.Enabled := True;
end;

{ IISCustomForm_CreatePage }

function CheckCustomForm_CreatePage(PreviousPageId: Integer): TWizardPage;
begin

  CheckPage := CreateCustomPage(
    PreviousPageId,
    'Installation options.',
    'NOTE: All installation parts must be completed for'#13'a full setup of LS Omni Server.'
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
    Caption :=  'Existing sql objects and Web Application get recreated.';
    Left := ScaleX(5);
    Top := CheckPage_lblComment.Top + CheckPage_lblComment.Height ;
    Width := ScaleX(400);
    Height := ScaleY(15);
    Enabled := True;
  end;

   { CheckPage_SQLCheckBox }
  CheckPage_SQLCheckBox := TCheckBox.Create(CheckPage);
  CheckPage_SQLCheckBox.Width := CheckPage.SurfaceWidth;
  CheckPage_SQLCheckBox.Height := ScaleY(34);
  CheckPage_SQLCheckBox.Caption := 'Create LSOmni SQL Server database';
  CheckPage_SQLCheckBox.Checked := True;
  CheckPage_SQLCheckBox.Parent := CheckPage.Surface;
  //CheckPage_SQLCheckBox.OnClick := @ChkOnChange;
  CheckPage_SQLCheckBox.Top := CheckPage_lblComment.Top + CheckPage_lblComment.Height + 15;

  CheckPage_NavSQLCheckBox := TCheckBox.Create(CheckPage);
  CheckPage_NavSQLCheckBox.Width := CheckPage.SurfaceWidth;
  CheckPage_NavSQLCheckBox.Height := ScaleY(34);
  CheckPage_NavSQLCheckBox.Caption := 'Configure LS Nav/Central SQL parameters';
  CheckPage_NavSQLCheckBox.Checked := True;
  CheckPage_NavSQLCheckBox.Parent := CheckPage.Surface;
  CheckPage_NavSQLCheckBox.Enabled := True;
  CheckPage_NavSQLCheckBox.Visible := True;
  //CheckPage_NavSQLCheckBox.OnClick := @ChkOnChange;
  CheckPage_NavSQLCheckBox.Top := CheckPage_SQLCheckBox.Top + CheckPage_SQLCheckBox.Height;

  CheckPage_IISCheckBox := TCheckBox.Create(CheckPage);
  CheckPage_IISCheckBox.Width := CheckPage.SurfaceWidth;
  CheckPage_IISCheckBox.Height := ScaleY(34);
  CheckPage_IISCheckBox.Caption := 'Create LSOmniService WCF service under IIS 7+';
  CheckPage_IISCheckBox.Checked := True;
  CheckPage_IISCheckBox.Parent := CheckPage.Surface;
  //CheckPage_IISCheckBox.OnClick := @ChkOnChange;
  CheckPage_IISCheckBox.Top := CheckPage_NavSQLCheckBox.Top + CheckPage_NavSQLCheckBox.Height;

    { CheckPage_lblComment }
  CheckPage_lblComment2 := TLabel.Create(CheckPage);
  with CheckPage_lblComment2 do
  begin
    Parent := CheckPage.Surface;
    Caption :=  'The LSOmniService is created as a Web Application.'#13'You must have administration rights (sysadmin for sql server).'#13
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


