import { AppProps } from "index";
import React from "react";
import { notifyToast } from "utils";
import { ViewContainer } from "../Views";

interface Props extends AppProps{
  onViewChangeHandler(view: string): void;
  nonce: boolean;
}
interface State {}

export class PasswordReset extends React.Component<Props, State> {
  resetPassword = async ({
    password,
    confirmPassword
  }: {
    password: string;
    confirmPassword: string;
  }) => {
    const { history, nonce, actionFetch } = this.props;

    if (password !== confirmPassword) {
      notifyToast("error", "Passwords do not match");
      return;
    }

    await actionFetch("users", "CHANGE_PASSWORD", { password, nonce });
    notifyToast("info", "Your password has been reset, please login");
    history.push("/");
  };

  btmItems = [
    {
      content: "Back to Login",
      className: "link",
      onClick: () => this.props.onViewChangeHandler("signin")
    }
  ];

  formItems = [
    { name: "password", placeholder: "New Password", icon: "key", type: "password" },
    { name: "confirmPassword", placeholder: "Confirm Password", icon: "key", type: "password" }
  ];

  render() {
    return (
      <ViewContainer
        items={this.btmItems}
        className="password-reset"
        onSubmit={this.resetPassword}
        submitBtnTitle="Reset Password"
        header={{ title: "Password Reset" }}
      />
    );
  }
}

export default PasswordReset;
