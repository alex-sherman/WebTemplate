import React from "react";
import { ViewContainer } from "../Views";
import { notifyToast } from "utils";
import { Input } from "components/UI";
import { AppProps } from "index";
import { URLS } from "constants/URLS";

interface Props extends AppProps  {
  onViewChangeHandler(view: string): void;
}
interface State {}

class Register extends React.Component<Props, State> {
  onCreateAccount = async ({ name, password, confirmPassword }: { [s: string]: string }) => {
    if (!name || !password || !confirmPassword) {
      const missing = [] as Array<string>;
      if (!name) missing.push("Username");
      if (!password) missing.push("Password");
      if (!confirmPassword) missing.push("Confirm Password");
      notifyToast("error", `Missing ${missing.join(", ")}`);
      return;
    }

    // Validate password and confirmPassword
    if (password === confirmPassword) {
      const queryParams = { name, password };
      await this.props.urlFetch(URLS.users.ADD, queryParams).then((result: any) => {
        this.props.onLoginHandler(result);
      });
    } else {
      notifyToast("error", "Passwords do not match");
    }
  };

  btmItems = [
    {
      content: "Already Registered? Login Here",
      className: "link",
      onClick: () => this.props.onViewChangeHandler("signin")
    }
  ];

  render() {
    return (
      <ViewContainer
        items={this.btmItems}
        className="register"
        onSubmit={this.onCreateAccount}
        submitBtnTitle="Create Account"
        header={{ title: "Create a New Account" }}
      >
        <Input id="name" placeholder="Username" icon="user" />
        <Input id="password" placeholder="Password" icon="key" type="password" />
        <Input id="confirmPassword" placeholder="Confirm Password" icon="key" type="password" />
      </ViewContainer>
    );
  }
}

export default Register;
