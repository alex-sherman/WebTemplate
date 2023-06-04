import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { ViewContainer } from "../Views";
import { notifyToast } from "utils";
import { Input } from "components/UI";
import { AppProps } from "index";
import { URLS } from "constants/URLS";

interface Props extends AppProps {
  onViewChangeHandler(view: string): void;
}
interface State {}

class SignIn extends React.Component<Props, State> {
  onError = (error) => {
    if (error.status === 404) {
      notifyToast("error", "Invalid username/password");
      return true;
    }
    return false;
  };
  onSubmit = async ({ name, password }: { name: string; password: string }) => {
    if (!name || !password) {
      const missing = [] as Array<string>;
      if (!name) missing.push("Username");
      if (!password) missing.push("Password");
      notifyToast("error", `Missing ${missing.join(", ")}`);
      return;
    }

    let form = new FormData();
    form["LoginForm[username]"] = name;
    form["LoginForm[password]"] = password;
    form["yt0"] = "Login";

    await this.props.urlFetch(URLS.users.LOGIN, form, this.onError).then((result: any) => {
      this.props.onLoginHandler(result);
    });
  };

  btmItems = [
    {
      content: "Register",
      className: "link",
      onClick: () => this.props.onViewChangeHandler("register"),
    },
    {
      content: <FontAwesomeIcon icon={"circle"} size="sm" />,
      className: "signin-bottom-circle flex center",
    },
    {
      content: "Forgot Password?",
      className: "link",
      onClick: () => this.props.onViewChangeHandler("forgot"),
    },
  ];

  render() {
    return (
      <ViewContainer
        items={this.btmItems}
        className="signin"
        onSubmit={this.onSubmit}
        submitBtnTitle="Login"
        header={{ title: "UI", className: "app-title" }}
      >
        <Input id="name" placeholder="Username" icon="user" />
        <Input id="password" placeholder="Password" icon="key" type="password" />
      </ViewContainer>
    );
  }
}

export default SignIn;
