import React from "react";
import { SignIn, Register } from "./Views";
import "./Login.scss";
import { AppProps } from "index";
import { Redirect } from "react-router-dom";

interface State {
  view: string;
}

class Login extends React.Component<AppProps, State> {
  state = {
    view: "signin",
  };

  onViewChangeHandler = (view: string) => {
    this.setState({ view });
  };

  render() {
    let { view } = this.state;
    const { token, query: { redirect } } = this.props;
    if (token) return <Redirect to={redirect || "/"} />;
    return (
      <div id="login" className="flex grow center">
        {view === "signin" && (
          <SignIn onViewChangeHandler={this.onViewChangeHandler} {...this.props} />
        )}
        {view === "register" && (
          <Register onViewChangeHandler={this.onViewChangeHandler} {...this.props} />
        )}
      </div>
    );
  }
}

export default Login;
