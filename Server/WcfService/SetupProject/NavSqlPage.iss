[Code]

var
  NavSQLPage_lblNavCompany: TLabel;
  NavSQLPage_lblServer: TLabel;
  NavSQLPage_lblAuthType: TLabel;
  NavSQLPage_lblUser: TLabel;
  NavSQLPage_lblPassword: TLabel;
  NavSQLPage_lblDatabase: TLabel;
  NavSQLPage_lblNavVersion: TLabel;
  NavSQLPage_chkSQLAuth: TRadioButton;
  NavSQLPage_chkWindowsAuth: TRadioButton;
  NavSQLPage_ConnectButton : TButton;
  NavSQLPage_VerCombBox: TNewComboBox; 
  
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
    case NavSQLPage_VerCombBox.ItemIndex of
      0: ADOCommand.CommandText := 'SELECT [Local Store No_] FROM [' + company + '$Retail Setup]';
      1: ADOCommand.CommandText := 'SELECT [Local Store No_] FROM [' + company + '$Retail Setup$5ecfc871-5d82-43f1-9c54-59685e82318d]';
      2: ADOCommand.CommandText := 'SELECT [Local Store No_] FROM [' + company + '$LSC Retail Setup$5ecfc871-5d82-43f1-9c54-59685e82318d]';
    end;      

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
    'LS Nav/LS Central SQL Server Database',
    'Configure SQL connection parameters and permissions to access LS Nav/LS Central Database'
  );
 
  { lblServer }
  NavSQLPage_lblServer := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblServer do
  begin
    Parent := NavSQLPage.Surface;
    Caption :=  'SQL Server instance:';
    Left := ScaleX(24);
    Top := ScaleY(6);
    Width := ScaleX(130);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { txtServer }
  NavSQLPage_txtServer := TEdit.Create(NavSQLPage);
  with NavSQLPage_txtServer do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(3);
    Width := ScaleX(225);
    Height := ScaleY(21);
    TabOrder := 1;
    Enabled := True;
    OnChange := @NavSQLServerOnChange;
    ShowHint := True;
    Hint := 'SQL Server name and instance where LS Central Database is located.';
  end;

  { lblDatabase }
  NavSQLPage_lblDatabase := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblDatabase do
  begin
    Parent := NavSQLPage.Surface;
    Caption := 'SQL Database name:';
    Left := ScaleX(24);
    Top := ScaleY(28);
    Width := ScaleX(130);
    Height := ScaleY(13);
    Enabled := True;
  end;
  { txtDBname }
  NavSQLPage_txtDBname := TEdit.Create(NavSQLPage);
  with NavSQLPage_txtDBname do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(25);
    Width := ScaleX(225);
    Height := ScaleY(21);
    Enabled := True;
    OnChange := @NavSQLServerOnChange;
    TabOrder := 2;
    ShowHint := True;
    Hint := 'LS Central Database name.';
  end;

  { lblNavCompany }
  NavSQLPage_lblNavCompany := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblNavCompany do
  begin
    Parent := NavSQLPage.Surface;
    Caption := 'Company name:';
    Left := ScaleX(24);
    Top := ScaleY(50);
    Width := ScaleX(140);
    Height := ScaleY(13);
  end;
  { NavSQLPage_txtNavCompany }
  NavSQLPage_txtNavCompany := TEdit.Create(NavSQLPage);
  with NavSQLPage_txtNavCompany do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(47);
    Width := ScaleX(225);
    Height := ScaleY(21);
    Enabled := True;
    OnChange := @NavSQLServerOnChange;
    TabOrder := 3;
    ShowHint := True;
    Hint := 'LS Central Company Name to connect to.';
  end;

  { lblNavVersion }
  NavSQLPage_lblNavVersion := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblNavVersion do
  begin
    Parent := NavSQLPage.Surface;
    Caption := 'Version range:';
    Left := ScaleX(24);
    Top := ScaleY(72);
    Width := ScaleX(140);
    Height := ScaleY(13);
  end;
  { NavSQLPage_VerCombBox }
  NavSQLPage_VerCombBox := TNewComboBox.Create(NavSQLPage);
  with NavSQLPage_VerCombBox do
  begin
    Parent := NavSQLPage.Surface;
    Left := ScaleX(152);
    Top := ScaleY(69);
    Width := ScaleX(225);
    Height := ScaleY(18);
    ShowHint := True;
    Hint := 'Select the LS Central version range that is currently being used.';
    Style := csDropDownList;
    Items.Add('LS Nav 14 & earlier');
    Items.Add('LS Central 15-17.4');
    Items.Add('LS Central 17.5 and later');
  end;
  
  { lblAuthType }
  NavSQLPage_lblAuthType := TLabel.Create(NavSQLPage);
  with NavSQLPage_lblAuthType do
  begin
    Parent := NavSQLPage.Surface;
    Caption :=  'Log on credentials';
    Left := ScaleX(24);
    Top := ScaleY(97);
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
    Top := ScaleY(113);
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
    Top := ScaleY(133);
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
    Top := ScaleY(156);
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
    Top := ScaleY(153);
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
    Top := ScaleY(180);
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
    Top := ScaleY(177);
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
      Top := ScaleY(207);
      Width := ScaleX(141);
      Height := ScaleY(21);
      Enabled := False;
      TabOrder := 8;
      Caption := 'Test SQL Connection';
      OnClick := @NavSQLPageTestConnectionClick;
      ShowHint := True;
      Hint := 'Test Connection to LS Central database. NOTE: LSCommerceUser does not exist at this moment so Test Connection may not work until after Install.';
    end;

  //does not work except from main form
  with NavSQLPage do
  begin
    OnActivate := @NavSQLCustomForm_Activate;
    OnNextButtonClick := @NavSQLCustomForm_NextButtonClick;
  end;

  Result := NavSQLPage;
end;
