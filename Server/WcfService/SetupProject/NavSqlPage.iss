[Code]

var
  NavSQLPage_lblNavCompany: TLabel;
  NavSQLPage_lblServer: TLabel;
  NavSQLPage_lblAuthType: TLabel;
  NavSQLPage_lblUser: TLabel;
  NavSQLPage_lblPassword: TLabel;
  NavSQLPage_lblDatabase: TLabel;
  NavSQLPage_chkSQLAuth: TRadioButton;
  NavSQLPage_chkWindowsAuth: TRadioButton;
  NavSQLPage_ConnectButton : TButton;
  NavSQLPage_V15CheckBox: TCheckBox; 
  
  NavSQLPage_txtServer: TEdit;
  NavSQLPage_txtUsername: TEdit;
  NavSQLPage_txtPassword: TPasswordEdit;
  NavSQLPage_txtDBname: TEdit;
  NavSQLPage_txtNavCompany: TEdit;

  NavSQLPage: TWizardPage;


// enable/disable child text boxes & functions when text has been entered into Server textbox. Makes no sense to populate child items unless a value exists for server.
Procedure NavSQLServerOnChange (Sender: TObject);
begin 
  Log('NavSQLServerOnChange called');
  WizardForm.NextButton.Enabled := False;
  if (Length(NavSQLPage_txtDBname.Text) > 0) and (Length(NavSQLPage_txtServer.Text) > 0) and (Length(NavSQLPage_txtNavCompany.Text) > 0) then
  begin
    WizardForm.NextButton.Enabled := True;
    NavSQLPage_ConnectButton.Enabled := True;
  end;
                             
  if Length(NavSQLPage_txtServer.Text) > 0 then
  begin
    NavSQLPage_lblAuthType.Enabled := True;
    NavSQLPage_chkWindowsAuth.Enabled := True;
    NavSQLPage_chkSQLAuth.Enabled := True;
  end
  else
  begin
    NavSQLPage_lblAuthType.Enabled := False;
    NavSQLPage_chkWindowsAuth.Enabled := False;
    NavSQLPage_chkSQLAuth.Enabled := False;
  end
end;

// enable/disable user/pass text boxes depending on selected auth type. A user/pass is only required for SQL Auth
procedure  NavSQLAuthOnChange (Sender: TObject);
begin
  Log('NavSQLAuthOnChange called');
  if NavSQLPage_chkWindowsAuth.Checked then
  begin
    NavSQLPage_lblUser.Enabled := false;
    NavSQLPage_lblPassword.Enabled := false;
    NavSQLPage_txtUsername.Enabled := false;
    NavSQLPage_txtPassword.Enabled := false;
  end
  Else
  begin
    NavSQLPage_lblUser.Enabled := true;
    NavSQLPage_lblPassword.Enabled := true;
    NavSQLPage_txtUsername.Enabled := true;
    NavSQLPage_txtPassword.Enabled := true;
  end
end;

function NavSQLPageADOTestConnection(dbServer: string; dbname: string; userName: string; pwd: string; company: string; windowsAuth: Boolean): bool;
var  
  ADOConnection: Variant;
  ADOCommand: Variant;
  ADORecordset: Variant;
begin
  Log('NavSQLPageADOTestConnection called');

  try
    dbServer := Trim(dbServer);
    userName := Trim(userName);
    pwd := Trim(pwd);
  
    // create the ADO connection object
    ADOConnection := CreateOleObject('ADODB.Connection');
    // build a connection string; 
    ADOConnection.ConnectionString := 'Provider=SQLOLEDB;Data Source=' + dbServer + ';Initial Catalog=' + dbname + ';Application Name=My Execute SQL;';

    if windowsAuth then
      ADOConnection.ConnectionString := ADOConnection.ConnectionString + 'Integrated Security=SSPI;'         // Windows Auth
    else
      ADOConnection.ConnectionString := ADOConnection.ConnectionString + 'User Id=' + userName + ';' + 'Password=' + pwd + ';';

    Log('NavSQLPageADOTestConnection string: ' + ADOConnection.ConnectionString);
    ADOConnection.Open;

	ADOCommand := CreateOleObject('ADODB.Command');
	ADOCommand.ActiveConnection := ADOConnection;
	if NavSQLPage_V15CheckBox.Checked then
      ADOCommand.CommandText := 'SELECT [Local Store No_] FROM [' + company + '$Retail Setup$5ecfc871-5d82-43f1-9c54-59685e82318d]'
	else
      ADOCommand.CommandText := 'SELECT [Local Store No_] FROM [' + company + '$Retail Setup]';

	ADOCommand.CommandType := adCmdText;
    Log('NavSQLPageADOTestConnection query: ' + ADOCommand.CommandText);
	ADORecordset := ADOCommand.Execute;
    ADOConnection.Close;
    MsgBox('Success '#13#13 'Connected !', mbInformation, MB_OK);
  except
    MsgBox(GetExceptionMessage, mbError, MB_OK);
  end;

  Result := true;
end;

procedure NavSQLPageTestConnectionClick(Sender: TObject);
begin
  Log('NavSQLPageTestConnectionClick called');
  NavSQLPageADOTestConnection(NavSQLPage_txtServer.Text, NavSQLPage_txtDBname.Text, NavSQLPage_txtUsername.Text, NavSQLPage_txtPassword.Text, NavSQLPage_txtNavCompany.Text, NavSQLPage_chkWindowsAuth.Checked);
end;

{ SQLCustomForm_NextButtonClick }
// try to connect to supplied db. Don't need to catch errors/close conn on error because a failed connection is never opened.
function NavSQLCustomForm_NextButtonClick(Page: TWizardPage): Boolean;
begin
  Log('NavSQLCustomForm_NextButtonClick called');
  Result := True;
end;

procedure NavSQLCustomForm_Activate(Page: TWizardPage) ;
begin
  Log('NavSQLCustomForm_Activate called');
  WizardForm.NextButton.Enabled := False;

  if (Length(NavSQLPage_txtDBname.Text) > 0) and (Length(NavSQLPage_txtServer.Text) > 0) and (Length(NavSQLPage_txtNavCompany.Text) > 0) then
  begin
    WizardForm.NextButton.Enabled := True;
    NavSQLPage_ConnectButton.Enabled := True;
  end;
end;

{ SQLCustomForm_CreatePage }
function NavSQLCustomForm_CreatePage(PreviousPageId: Integer): TWizardPage;
begin
  NavSQLPage := CreateCustomPage(
    PreviousPageId,
    'LS Nav/LS Central SQL Server Database used by the LS Commerce Service',
    'Configures SQL parameters and permissions in the LS Nav/LS Central Database'
  );
 
  { lblServer }
  NavSQLPage_lblServer := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblServer do
  begin
    Parent := NavSQLPage.Surface;
    Caption :=  'SQL Server instance:';
    Left := ScaleX(24);
    Top := ScaleY(11);
    Width := ScaleX(130);
    Height := ScaleY(13);
    Enabled := True;
    ShowHint := True;
    Hint     := 'SQLServer or SQLServer\NAVDEMO';
  end;
  { txtServer }
  NavSQLPage_txtServer := TEdit.Create(NavSQLPage);
  with NavSQLPage_txtServer do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(175);
    Top := ScaleY(8);
    Width := ScaleX(225);
    Height := ScaleY(21);
    TabOrder := 1;
    Enabled := True;
    OnChange := @NavSQLServerOnChange;
    ShowHint := True;
    Hint     := 'SQLServer or SQLServer\NAVDEMO';
  end;

  { lblDatabase }
  NavSQLPage_lblDatabase := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblDatabase do
  begin
    Parent := NavSQLPage.Surface;
    Caption := 'SQL Database name:';
    Left := ScaleX(24);
    Top := ScaleY(33);
    Width := ScaleX(130);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { txtDBname }
  NavSQLPage_txtDBname := TEdit.Create(NavSQLPage);
  with NavSQLPage_txtDBname do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(175);
    Top := ScaleY(30);
    Width := ScaleX(225);
    Height := ScaleY(21);
    Enabled := True;
    OnChange := @NavSQLServerOnChange;
    TabOrder := 2;
  end;

  { lblNavCompany }
  NavSQLPage_lblNavCompany := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblNavCompany do
  begin
    Parent := NavSQLPage.Surface;
    Caption := 'Company name:';
    Left := ScaleX(24);
    Top := ScaleY(55);
    Width := ScaleX(140);
    Height := ScaleY(13);
  end;
  { NavSQLPage_txtNavCompany }
  NavSQLPage_txtNavCompany := TEdit.Create(NavSQLPage);
  with NavSQLPage_txtNavCompany do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(175);
    Top := ScaleY(52);
    Width := ScaleX(225);
    Height := ScaleY(21);
    Enabled := True;
    OnChange := @NavSQLServerOnChange;
    TabOrder := 3;
  end;

  { NavSQLPage_V15CheckBox }
  NavSQLPage_V15CheckBox := TCheckBox.Create(NavSQLPage);
  with NavSQLPage_V15CheckBox do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(175);
    Top := ScaleY(74);
    Width := NavSQLPage.SurfaceWidth;
    Height := ScaleY(18);
    Caption := 'LS Central 15 or later';
    Checked := False;
  end;

  { lblAuthType }
  NavSQLPage_lblAuthType := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblAuthType do
  begin
    Parent := NavSQLPage.Surface;
    Caption :=  'Log on credentials';
    Left := ScaleX(24);
    Top := ScaleY(92);
    Width := ScaleX(87);
    Height := ScaleY(13);
    Enabled := True;
  end;

  { chkWindowsAuth }
  NavSQLPage_chkWindowsAuth := TRadioButton.Create(NavSQLPage);
  with NavSQLPage_chkWindowsAuth do
  begin
    Parent := NavSQLPage.Surface;
    Caption := 'Use Windows Authentication';
    Left := ScaleX(32);
    Top := ScaleY(108);
    Width := ScaleX(177);
    Height := ScaleY(17);
    Checked := False;
    TabOrder := 4;
    TabStop := True;
    OnClick := @NavSQLAuthOnChange;
    Enabled := True;
  end;

  { chkSQLAuth }
  NavSQLPage_chkSQLAuth := TRadioButton.Create(NavSQLPage);
  with NavSQLPage_chkSQLAuth do
  begin
    Parent := NavSQLPage.Surface;
    Caption := 'Use SQL Server Authentication';
    Left := ScaleX(32);
    Top := ScaleY(128);
    Width := ScaleX(185);
    Height := ScaleY(17);
    Checked := True;
    TabOrder := 5;
    OnClick := @NavSQLAuthOnChange;
    Enabled := True;
  end;

  { lblUser }
  NavSQLPage_lblUser := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblUser do
  begin
    Parent := NavSQLPage.Surface;
    Caption := 'User (dbowner):' ;
    Left := ScaleX(60);
    Top := ScaleY(151);
    Width := ScaleX(85);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { txtUsername }
  NavSQLPage_txtUsername := TEdit.Create(NavSQLPage);
  with NavSQLPage_txtUsername do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(148);
    Width := ScaleX(225);
    Height := ScaleY(21);
    Enabled := True;
    TabOrder := 6;
  end;

  { lblPassword }
  NavSQLPage_lblPassword := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblPassword do
  begin
    Parent := NavSQLPage.Surface;
    Caption := 'Password:' ;
    Left := ScaleX(60);
    Top := ScaleY(175);
    Width := ScaleX(85);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { txtPassword }
  NavSQLPage_txtPassword := TPasswordEdit.Create(NavSQLPage);
  with NavSQLPage_txtPassword do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(172);
    Width := ScaleX(225);
    Height := ScaleY(21);
    Enabled := True;
    TabOrder := 7;
  end;

  { NavSQLPage_ConnectButton }
  NavSQLPage_ConnectButton := TButton.Create(NavSQLPage);
    with NavSQLPage_ConnectButton do
    begin
      Parent := NavSQLPage.Surface;
      Left := ScaleX(145);
      Top := ScaleY(202);
      Width := ScaleX(141);
      Height := ScaleY(21);
      Enabled := False;
      TabOrder := 8;
      Caption := 'Test SQL Connection';
      OnClick := @NavSQLPageTestConnectionClick;
    end;

  //does not work except from main form
  with NavSQLPage do
  begin
    OnActivate := @NavSQLCustomForm_Activate;
    OnNextButtonClick := @NavSQLCustomForm_NextButtonClick;
  end;

  Result := NavSQLPage;
end;
